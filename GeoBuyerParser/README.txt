For project docker file
docker run -d --name geo-buyer-parser-db geo-buyer-parser-db-image
docker run -d -p 8080:80 --name geo-buyer-parser --link geo-buyer-parser-db geo-buyer-parser-image

To access the DB
docker run -d --name geo-buyer-parser-db -v /path/on/host:/var/lib/sqlite geo-buyer-parser-db-image

