cd "$PSScriptRoot" 


$src = "$env:appdata\Markdown Monster\Addins\SaveImageToAzureBlobStorage"
$tgt = "..\build"

"Cleaning up build files..."
del $tgt\addin.zip

remove-item -recurse -force $tgt\Distribution
md $tgt\Distribution

"Copying files..."
copy $src\*.dll $tgt\Distribution
copy $src\version.json $tgt\Distribution
copy $src\version.json $tgt\

"Zipping up setup file..."
7z a -tzip  $tgt\addin.zip $tgt\Distribution\*.*
