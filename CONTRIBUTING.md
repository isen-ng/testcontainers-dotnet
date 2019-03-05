# Development requirements

* dotnet-sdk 2.1
* docker

```
# build
dotnet build

# test
dotnet test

# Publishing nuget package to local feed
./local-publish.sh
```

# Windows development guide (on linux)

```
# create windows docker-machine
git clone https://github.com/StefanScherer/windows-docker-machine
cd windows-docker-machine
vagrant up --provider virtualbox 2016-box

# List your new docker-machine
docker-machine ls

# Switch to windows docker-machine
eval $(docker-machine env 2016-box)

# Switch back to linux docker
eval $(docker-machine env -unset)
```

### Windows current development [Help needed]

Goals

* Reuse existing ryuk go code
* Try to reuse existing Makefile
* Use `golang:1.10-nanoserver` as the build container
* Use `mcr.microsoft.com/windows/nanoserver:sac2016` as the host container
  * AppVeyor CI still uses 2016
* Publish windows version of Ryuk using code from linux Ryuk
  * Keep code the same by using git submodules

Tried so far

* nanoserver does not have .net framework or .net core installed
* cannot run make / git without .net dlls
* cannot install chocolatey because no .net dlls (it uses System.net.WebClient)
* going to try to install cygwin

### Powershell gists

```
# wget equivalent
Invoke-WebRequest https://github.com/isen-ng/testcontainers-dotnet/archive/master.zip -OutFile code.zip
Expand-Archive -Path .\code.zip -DestinationPath . 

# Make for windows:
http://gnuwin32.sourceforge.net/packages/make.htm
Invoke-WebRequest https://nchc.dl.sourceforge.net/project/gnuwin32/make/3.81/make-3.81-bin.zip -OutFile make.zip
Expand-Archive -Path .\make.zip -DestinationPath . 
```