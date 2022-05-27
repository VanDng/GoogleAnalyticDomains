# GoogleAnalyticDomains

# Instruction

```
docket network create network-analysis

docker build --progress=plain --no-cache -t sample-app -f Dockerfile .
docker run -it --name sample-app-container --network network-analysis --rm sample-app  

docker run --name tcpdump -d --rm --net container:sample-app-container -v $PWD/Docker_TCPDump:/tcpdump kaazing/tcpdump:latest -vv -i any -w /tcpdump/tcpdump.pcap -U --immediate-mode
```


https://github.com/kaazing/dockerfiles/blob/master/tcpdump/Dockerfile