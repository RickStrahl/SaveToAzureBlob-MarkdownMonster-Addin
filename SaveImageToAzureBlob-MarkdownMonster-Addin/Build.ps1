cd "$PSScriptRoot" 


$src = "$env:appdata\Markdown Monster\Addins\SaveImageToAzureBlobStorage"
$tgt = "..\build"

"Cleaning up build files..."
if(Test-Path $tgt -PathType Container) {

   del $tgt\addin.zip
   remove-item -recurse -force $tgt\Distribution
}
md $tgt\Distribution

"Copying files..."
copy $src\*.dll $tgt\Distribution

"Removing not needed files..."
#del $tgt\Distribution\MarkdownMonster.dll
#del $tgt\Distribution\FontAwesome.WPF.dll
#del $tgt\Distribution\MarkDig.dll
#del $tgt\Distribution\NHunspell.dll
#del $tgt\Distribution\MahApps.Metro.dll
#del $tgt\Distribution\Westwind.Utilities.dll
#del $tgt\Distribution\System.Windows.Interactivity.dll

"Updating version..."
copy $src\version.json $tgt\Distribution
copy $src\version.json $tgt\
copy $src\version.json $tgt\Distribution

"Zipping up setup file..."
.\7z a -tzip  $tgt\addin.zip $tgt\Distribution\*.*

remove-item $tgt\Distribution -recurse