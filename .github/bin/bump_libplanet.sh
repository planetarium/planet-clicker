#!/bin/bash

set -ev

clone_libplanet() {
    parent_directory="$1"

    echo "$parent_directory" > /dev/stderr
    libplanet_git_directory="$parent_directory/libplanet-$RANDOM"
    echo "$libplanet_git_directory" > /dev/stderr
    mkdir -p "$libplanet_git_directory"

    pushd "$parent_directory" > /dev/null
        git clone https://github.com/planetarium/libplanet "$libplanet_git_directory"
    popd > /dev/null

    echo "$libplanet_git_directory"
}

get_latest_tag() {
    git_directory="$1"
    pushd "$git_directory" > /dev/null
        echo $(git tag --list --sort creatordate | tac | head -n 1)
    popd > /dev/null
}

checkout() {
    git_directory="$1"
    tag="$2"

    pushd "$git_directory" > /dev/null
        git checkout "$tag"
    popd > /dev/null
}

dotnet_publish() {
    dotnet_directory="$1"

    output_directory="$dotnet_directory/out"

    dotnet publish "$dotnet_directory" -r -win-x64 -o "$output_directory" -f netstandard2.0 > /dev/stderr

    echo "$output_directory"
}

copy_libplanet_files() {
    source_directory="$1"
    target_directory="$2"

    for file in `find $source_directory/*.{dll,xml}`; do
        echo "$file"
        cp "$file" "$target_directory"
    done
}

libplanet_git_directory=$(clone_libplanet "/tmp")
latest_tag=$(get_latest_tag "$libplanet_git_directory")
checkout "$libplanet_git_directory" "$latest_tag"

libplanet_project_directory="$libplanet_git_directory/Libplanet"
libplanet_publish_directory=$(dotnet_publish "$libplanet_project_directory")

LIBPLANET_UNITY_DIRECTORY="$(pwd)/planet-clicker/Assets/LibplanetUnity/Packages"

echo "$libplanet_publish_directory"
copy_libplanet_files "$libplanet_publish_directory" "$LIBPLANET_UNITY_DIRECTORY"

echo "::set-output name=libplanet_latest_tag::$latest_tag"
