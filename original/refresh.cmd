xcopy ..\..\grin grin /Y /H /S
rmdir grin\.git /s /q
rmdir grin\.hooks /s /q
del grin\.gitignore

xcopy ..\..\rust-secp256k1-zkp rust-secp256k1-zkp /Y /H /S
rmdir rust-secp256k1-zkp\.git /s /q
del rust-secp256k1-zkp\.gitignore
del rust-secp256k1-zkp\.gitmodules
