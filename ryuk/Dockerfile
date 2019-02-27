FROM golang:1.10-nanoserver as workspace
WORKDIR /go/src/github.com/testcontainers/moby-ryuk

COPY make-3.81-bin.zip ./make
RUN cd make 
RUN unzip make 
RUN new-item alias:make -value "$pwd\bin\make.exe"

COPY glide.lock glide.yaml Makefile ./
RUN make bootstrap
COPY . .
RUN make build

FROM mcr.microsoft.com/windows/nanoserver:sac2016
RUN apk --no-cache add ca-certificates
COPY --from=workspace /go/src/github.com/testcontainers/moby-ryuk/bin/moby-ryuk /app
CMD ["/app"]
