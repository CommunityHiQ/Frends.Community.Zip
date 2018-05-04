# Frends.Community.Zip
FRENDS community task for creating zip archive

- [Installing](#installing)
- [Tasks](#tasks)
  - [Create Archive](#createarchive)
- [License](#license)
- [Building](#building)
- [Contributing](#contributing)
- [Change Log](#change-log)

# Installing
You can install the task via FRENDS UI Task View or you can find the nuget package from the following nuget feed
'Insert nuget feed here'

# Tasks

## CreateArchive
The Zip.CreateArchive task meant for creating zip file from selected files and/or folders. Created zip file content can be flatten and file can be protected with password.

### Task Properties

#### Source

| Property             | Type                 | Description                          | Example |
| ---------------------| ---------------------| ------------------------------------ | ----- |
| Path | string | Source file(s) directory. | c:\source_folder\ |
| File mask | string | The search string to match against the names of files. Supports wildcards '?' and '*'. | * |
| Include sub folders | bool | Indicates if sub folders in Path property should be included in search with file mask. | false |

#### Destination

| Property             | Type                 | Description                          | Example |
| ---------------------| ---------------------| ------------------------------------ | ----- |
| Path | string | Directory for zip file created. | c:\destination_folder\ |
| File name | string | Name of zip file created. | example.zip |
| Password | string | If set, zip archive will be password protected. | |
| Flatten folders | bool | Choose if source folder structure should be flatten. | false |
| Rename duplicate files | bool | If source files contains duplicate names, they are renamed (example.txt --&gt; example_(1).txt) | true |

#### Options

| Property             | Type                 | Description                          | Example |
| ---------------------| ---------------------| ------------------------------------ | ----- |
| Throw error if no files found | bool | If no files were found in source path, throw exception. Otherwise returns object with FileCount = 0 | true |
| Destination file exists action | Enum {Error, Overwrite, Rename} | What to do if destination zip file already exists. | Rename (renames zip file: example.zip --&gt; example_(1).zip) |
| Create destination folder | bool | True: creates destination folder if it does not exist. False: throws error if destination folder is not found. | false |


### Result

| Property             | Type                 | Description                          | Example |
| ---------------------| ---------------------| ------------------------------------ | ----- |
| FileName | string | Name of the zip file created | 'my_zipfile.zip' |
| FilePath | string | Directory where zip file is saved | 'C:\my_zips\' |
| FileCount | int | Number of files added to zip archive | 10 |
| ArchivedFiles | List&lt;string&gt; | File names with relative path in zip archive | {'file_1.txt', 'file_2.txt', 'sub_folder/file_3.txt'} |

# License

This project is licensed under the MIT License - see the LICENSE file for details

# Building

Clone a copy of the repo

`git clone https://github.com/CommunityHiQ/Frends.Community.Zip.git`

Restore dependencies

`nuget restore frends.community.zip`

Rebuild the project

Run Tests with nunit3. Tests can be found under

`Frends.Community.Zip.Tests\bin\Release\Frends.Community.Zip.Tests.dll`

Create a nuget package

`nuget pack nuspec/Frends.Community.Zip.nuspec`

# Contributing
When contributing to this repository, please first discuss the change you wish to make via issue, email, or any other method with the owners of this repository before making a change.

1. Fork the repo on GitHub
2. Clone the project to your own machine
3. Commit changes to your own branch
4. Push your work back up to your fork
5. Submit a Pull request so that we can review your changes

NOTE: Be sure to merge the latest from "upstream" before making a pull request!

# Change Log

| Version             | Changes                 |
| ---------------------| ---------------------|
| 2.0.0 | Initial Frends.Community version of ZipTask converted from old Frends.Common code base |
| 2.1.0 | Renamed Task class and added Change log section to Readme |
| 2.2.0 | Downgraded Frends.Tasks.Attributes from 1.2.1 to 1.2.0, because of a bug in 1.2.1 |
| 2.3.0 | Changed target .net framework to 4.5.2. Replaced Frends.Task.Attributes with ComponentModel.DataAnnotations |
