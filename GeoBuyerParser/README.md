For project docker file
docker run -d --name geo-buyer-parser-db -v /path/on/host:/var/lib/sqlite valeriucoj/geo-buyer-parser-db
docker run -d -p 8080:80 --name geo-buyer-parser --link geo-buyer-parser-db valeriucoj/geo-buyer-parser


# via docker-compose
```
cd GeoBuyerParser
docker-compose up -d
```

# start parce product
```
http://0.0.0.0:8081/api/ParseProducts
```

#connect into container
```
docker exec -it geo-buyer-parser sh
```

#execute comman into container
```
docker exec geo-buyer-parser [:COMMAND]
```

