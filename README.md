# Offline Mapbook WPF

This repo provides an open source app called Offline Mapbook built in WPF with the [ArcGIS Runtime SDK for .NET](https://developers.arcgis.com/net/). With very little customization, you can change the app to use your own data. Screenshots of the app and more detailed documentation can be found on the [Developers Site](https://developers.arcgis.com/example-apps/mapbook-wpf/).

<!-- MDTOC maxdepth:6 firsth1:0 numbering:0 flatten:0 bullets:1 updateOnSave:1 -->

- [Features](#features)   
- [Detailed Documentation](#detailed-documentation)   
- [Development Instructions](#development-instructions)   
   - [Fork the repo](#fork-the-repo)   
   - [Clone the repo](#clone-the-repo)   
      - [Command line Git](#command-line-git)   
      - [Configuring a Remote for a Fork](#configuring-a-remote-for-a-fork)   
- [Requirements](#requirements)   
- [Resources](#resources)   
- [Issues](#issues)   
- [Contributing](#contributing)   
- [MDTOC](#mdtoc)   
- [Licensing](#licensing)   

<!-- /MDTOC -->
---

## Features

- Mobile map packages
- Feature layers
- Identify
- Table of Contents
- Legend
- Locators with autosuggestions
- Bookmarks
- Portal Authentication using IWA and OAuth2

## Detailed Documentation

Read the [docs](./docs/README.md) for a detailed explanation of the application, including its architecture and how it leverages the ArcGIS platform, as well as how you can begin using the app right away.

## Development Instructions

This Offline Mapbook repo is a Visual Studio 2017 Project that can be directly cloned and imported into Visual Studio 2017 or higher.

### Fork the repo

[Fork](https://github.com/Esri/mapbook-wpf/fork) the Offline Mapbook WPF repo.

### Clone the repo

Once you have forked the repo, you can make a clone.

#### Command line Git

1. [Clone](https://help.github.com/articles/fork-a-repo#step-2-clone-your-fork) the Offline Mapbook repo
2. cd into the ```mapbook-wpf``` folder
3. Make your changes and create a [pull request](https://help.github.com/articles/creating-a-pull-request)

#### Configuring a Remote for a Fork

If you make changes in the fork and would like to [sync](https://help.github.com/articles/syncing-a-fork/) those changes with the upstream repository, you must first [configure the remote](https://help.github.com/articles/configuring-a-remote-for-a-fork/). This will be required when you have created local branches and would like to make a [pull request](https://help.github.com/articles/creating-a-pull-request) to your upstream branch.

1. In the Terminal (for Mac users) or command prompt (fow Windows and Linux users) type ```git remote -v``` to list the current configured remote repo for your fork.
2. ```git remote add upstream https://github.com/Esri/mapbook-wpf.git``` to specify new remote upstream repository that will be synced with the fork. You can type ```git remote -v``` to verify the new upstream.

If there are changes made in the Original repository, you can sync the fork to keep it updated with upstream repository.

1. In the terminal, change the current working directory to your local project
2. Type ```git fetch upstream``` to fetch the commits from the upstream repository
3. ```git checkout master``` to checkout your fork's local master branch.
4. ```git merge upstream/master``` to sync your local master' branch withupstream/master. Note: Your local changes will be retained and your fork's master branch will be in sync with the upstream repository.

## Requirements

- [Visual Studio 2017 or higher](https://www.visualstudio.com/downloads/)
    - Visual Studio 2019 is required if you want to use the .NET Core version.
- [.NET Framework 4.6.1 or higher](https://www.microsoft.com/net/download)
- [ArcGIS Runtime SDK for .NET 100.10 or higher](https://developers.arcgis.com/net/)
- [ArcGIS Runtime Toolkit for WPF v100.10 or higher](https://www.esri.com/arcgis-blog/products/developers/uncategorized/toolkit-for-arcgis-runtime-sdk-for-net-now-available/)

## Resources

- [Offline Mapbook documentation on Developers Site](https://developers.arcgis.com/example-apps/mapbook-wpf/)
- [ArcGIS Runtime SDK for .NET Developers Site](https://developers.arcgis.com/net/)
- [ArcGIS Developer Blog](https://www.esri.com/arcgis-blog/developers/)
- [twitter@ArcGISRuntime](https://twitter.com/ArcGISRuntime)
- [twitter@esri](https://twitter.com/esri)

## Issues

Find a bug or want to request a new feature enhancement? Let us know by submitting an issue.

## Contributing

Anyone and everyone is welcome to [contribute](https://github.com/Esri/mapbook-wpf/blob/master/CONTRIBUTING.md). We do accept pull requests.

1. Get involved
2. Report issues
3. Contribute code
4. Improve documentation

## MDTOC

Generation of this and other documents' table of contents in this repository was performed using the [MDTOC package for Atom](https://atom.io/packages/atom-mdtoc).

## Licensing

Copyright 2017-2019 Esri

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at

[https://www.apache.org/licenses/LICENSE-2.0](https://www.apache.org/licenses/LICENSE-2.0)

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

A copy of the license is available in the repository's [LICENSE](https://github.com/Esri/mapbook-wpf/blob/master/LICENSE) file.

For information about licensing your deployed app, see [License your app](https://developers.arcgis.com/net/latest/ios/guide/license-your-app.htm).
