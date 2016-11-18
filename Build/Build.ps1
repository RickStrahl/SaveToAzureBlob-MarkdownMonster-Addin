cd "$PSScriptRoot" 

"Cleaning up build files..."
del SaveImageToAzureBlob-MarkdownMonster-Addin.zip

remove-item -recurse -force .\Distribution
md Distribution


"Copying files..."
copy ..\SaveImageToAzureBlob-MarkdownMonster-Addin\bin\Release\*.dll .\Distribution
del .\Distribution\NewtonSoft.Json.dll

"Zipping up setup file..."
7z a -tzip  SaveImageToAzureBlob-MarkdownMonster-Addin.zip .\Distribution\*.*
