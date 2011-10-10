param($installPath, $toolsPath, $package, $project)

#-----------------------------------------------------------------------------
#
# Install and uninstall scripts contributed by Ed Obeda
# Automates the addition and removal of the post-build step to call Afterthought
#
#-----------------------------------------------------------------------------

# Get the current Post Build Event cmds
$currentPostBuildCmds = $project.Properties.Item("PostBuildEvent").Value

$buildCmd = '"$(SolutionDir)packages\Afterthought.{0}\tools\Afterthought.Amender.exe" "$(TargetPath)"' -f $package.Version

# Append our post build command if it's not already there
if (!$currentPostBuildCmds.Contains($buildCmd))
{
	# Append it, but need to manage carriage return line feeds
	# $project.Properties.Item("PostBuildEvent").Value += $buildCmd
       
	$lines = [regex]::Split($currentPostBuildCmds,"\r\n")
	$cmds = ""

	# Walk each entry
	foreach($line in $lines)
	{
		if( $line.Length -gt 0 -and !$line.Contains('Afterthought.Amender')) # don't include empty lines or the afterthought command
		{
			$cmds += $line + "`r`n"  # Creating a noramlized list of commands
		}
	}
      
	$cmds += $buildCmd + "`r`n"  # Add ourself
      
	$project.Properties.Item("PostBuildEvent").Value = $cmds
}