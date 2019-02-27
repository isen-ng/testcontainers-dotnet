FROM golang:1.10-nanoserver as workspace
WORKDIR /go/src/github.com/testcontainers/moby-ryuk
COPY glide.lock glide.yaml make-bootstrap.bat ./
RUN make-bootstrap.bat
COPY . .
RUN make-build.bat

FROM mcr.microsoft.com/windows/nanoserver:sac2016
RUN apk --no-cache add ca-certificates
COPY --from=workspace /go/src/github.com/testcontainers/moby-ryuk/bin/moby-ryuk /app
CMD ["/app"]
