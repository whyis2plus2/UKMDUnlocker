#!/usr/bin/bash
PROJECT_NAME="UKMDUnlocker"
VERSION="0.3.1"
BUILD_DIR="./out"
TARGET="whyis2plus2-$PROJECT_NAME-$VERSION.zip"

mkdir -p $BUILD_DIR
dotnet build
cp ./bin/Debug/netstandard2.1/$PROJECT_NAME.dll $BUILD_DIR
cp ./bin/Debug/netstandard2.1/$PROJECT_NAME.pdb $BUILD_DIR
cp ./README.md ./out
cp ./LICENSE ./out
cp ./icon.png ./out
cp ./manifest.json ./out
7z a -y -tzip -r $TARGET $PWD/$BUILD_DIR/* | grep -Ev ".*ing|^$"
