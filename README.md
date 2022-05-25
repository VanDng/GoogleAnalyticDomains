# GoogleAnalyticDomains

# Instruction

```
docket network create network-analysis

docker build --progress=plain --no-cache -t sample-app -f Dockerfile .
docker run -it --rm counter-image --name sample-app-container --network network-analysis

docker kill wireshark | docker run --rm -d --name=wireshark --net=network-analysis -p 8080:3000 --cap-add=NET_ADMIN -e PUID=1000 -e PGID=1000 -e TZ=Europe/London -v /path/to/config:/config lscr.io/linuxserver/wireshark:latest

docker run --name tcpdump -d --rm --net container:sample-app-container -v $PWD/tcpdump:/tcpdump kaazing/tcpdump -vv -i any -w /tcpdump/tcpdump2.pcap -U --immediate-mode
```


https://github.com/kaazing/dockerfiles/blob/master/tcpdump/Dockerfile