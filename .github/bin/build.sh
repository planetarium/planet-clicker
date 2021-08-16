echo -n "$ULF" | base64 -d > /tmp/unity.ulf

/opt/unity/Editor/Unity \
    -quit \
    -batchmode \
    -logFile /dev/stdout \
    -nographics \
    -manualLicenseFile /tmp/unity.ulf

/opt/unity/Editor/Unity \
    -quit \
    -batchmode \
    -logFile /dev/stdout \
    -nographics \
    -projectPath planet-clicker
