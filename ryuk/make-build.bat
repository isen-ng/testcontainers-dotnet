@echo off

go vet .\...
	
go fmt .\...

CGO_ENABLED=0 
go build -i -v -ldflags '-s' -o .\bin\moby-ryuk .\
