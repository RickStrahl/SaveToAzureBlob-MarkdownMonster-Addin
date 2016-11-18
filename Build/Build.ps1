cd "$PSScriptRoot" 

"Copying files..."
del .\Distribution\*.*
copy ..\SaveImageToAzureBlob-MarkdownMonster-Addin\bin\Release\*.dll .\Distribution

del .\Distribution\NewtonSoft.Json.dll

"Zipping up setup file..."
7z a -tzip ".\SaveImageToAzureBlob-MarkdownMonster-Addin.zip" ".\Distribution\*.*" 

pause