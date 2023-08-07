For project docker file
docker run -d --name geo-buyer-parser-db -v /path/on/host:/var/lib/sqlite valeriucoj/geo-buyer-parser-db
docker run -d -p 8080:80 --name geo-buyer-parser --link geo-buyer-parser-db valeriucoj/geo-buyer-parser


