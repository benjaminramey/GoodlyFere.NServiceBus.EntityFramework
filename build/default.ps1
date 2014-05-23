properties {
	$solution_name = "GoodlyFere.NServiceBus.EntityFramework"
	$nuspec_file_name = "$solution_name.nuspec"
	$package_dlls = @{
		"net45" = "$solution_name\bin\Release\$solution_name.dll"
	}
	$version = "1.0.0"
	$git_tags = git describe --tags --long
	if ($git_tags)
	{
		$build_version = "$version." + $git_tags.split('-')[1]
	}
	else
	{
		$build_version = "$version.0"
	}
	$run_tests = $true
  
	$base_dir  = resolve-path ..\
	$src_dir = "$base_dir\src"
	$build_dir = "$base_dir\build"
	$package_dir = "$build_dir\package"  
	$tools_dir = "$build_dir\Tools"
  
	$sln_file = "$src_dir\$solution_name.sln"
	$xunit_console = "$tools_dir\xunit.console.clr4.exe"
}

Framework "4.0"

task default -depends Package

task Clean -depends Init {
	# remove build/package folders
	if (Test-Path $package_dir) { ri -force -recurse $package_dir }
	
	# clean project builds
	$projFiles = @(gci $src_dir "*.csproj" -Recurse)
	foreach ($pf in @($projFiles)) {
		cd $pf.Directory
		exec { msbuild $pf "/t:Clean" } "msbuild clean failed."
	}
	
	cd $base_dir
	# recreate build/package folders
	mkdir @($package_dir) | out-null
}

task Init {
	cls
}

task Compile -depends Clean {
	exec { msbuild $sln_file "/p:Configuration=Release" } "msbuild (release) failed."
}

task Test -depends Compile -precondition { return $run_tests } {
	$test_dlls = @(gci $src_dir "*.Tests.dll" -Recurse)
	
	foreach ($dll in @($test_dlls)) {
		cd $dll.Directory
		exec { & $xunit_console "$dll" } "xunit failed."
	}
	
	cd $base_dir
}

task Package -depends Compile, Test {
	cp "$src_dir\$nuspec_file_name" "$package_dir"
	
	mkdir "$package_dir\lib"
	foreach ($k in $package_dlls.Keys) {
		$file = $package_dlls[$k]
		$path = "$src_dir\" + $file
		mkdir "$package_dir\lib\$k"
		cp "$path" "$package_dir\lib\$k"
	}
	
	cd $base_dir
	
	$spec = [xml](get-content "$package_dir\$nuspec_file_name")
	$spec.package.metadata.version = $build_version
	$spec.Save("$package_dir\$nuspec_file_name")
	
	exec { nuget pack "$package_dir\$nuspec_file_name" -o $build_dir }
}

task Push -depends Package {
	cd $build_dir
	$package_name = gci *.nupkg
	nuget delete $solution_name $build_version -NonInteractive
	exec { nuget push $package_name }
}
