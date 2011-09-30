param($installPath, $toolsPath, $package, $project)

#-----------------------------------------------------------------------------
#
# Install and uninstall scripts contributed by Ed Obeda
# Automates the addition and removal of the post-build step to call Afterthought
#
#-----------------------------------------------------------------------------

# Get the current Post Build Event cmds
$currentPostBuildCmds = $project.Properties.Item("PostBuildEvent").Value

# Check to see if we need to delete anything
if ($currentPostBuildCmds.Contains('Afterthought.Amender'))
{
    $lines = [regex]::Split($currentPostBuildCmds,"\r\n")
    $cleanedCmds = ""
   
   # Walk each entry
   foreach($line in $lines)
   {
        if($line.Length -gt 0 -and !$line.Contains('Afterthought.Amender')) # don't include empty lines or the afterthought command
        {
            $cleanedCmds += $line + "`r`n"  # Creating a cleaned list of commands
        }
   }

  # Update the build event with the cleaned values
  $project.Properties.Item("PostBuildEvent").Value = $cleanedCmds
}