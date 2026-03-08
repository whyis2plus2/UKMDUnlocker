PROJECT_NAME := UKMDUnlocker
VERSION := 0.3.1

BUILD_DIR := out
RELEASE_TARGET := whyis2plus2-$(PROJECT_NAME)-$(VERSION).zip

all: release

build:
	@mkdir -p $(BUILD_DIR)
	dotnet build
	cp ./bin/Debug/netstandard2.1/$(PROJECT_NAME).dll ./out
	cp ./README.md ./out
	cp ./LICENSE ./out
	cp ./icon.png ./out
	cp ./manifest.json ./out

$(BUILD_DIR): build

release: build
	7z a -y -tzip -r $(RELEASE_TARGET) $(PWD)/$(BUILD_DIR)/*

clean:
	rm -rf bin obj $(BUILD_DIR) $(RELEASE_TARGET)