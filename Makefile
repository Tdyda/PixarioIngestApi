.PHONY: up down build rebuild logs ps restart \
        logs-api logs-worker logs-mysql logs-rabbitmq \
        restart-api restart-worker start-deps stop-deps
        
up:
	docker compose up -d

down:
	docker compose down
   
build:
	docker compose build
    
rebuild:
	docker compose down
	docker compose build --no-cache 

logs:
	docker compose logs -f
 
logs-api:
	docker compose logs -f api
 
logs-worker:
	docker compose logs -f worker
 
logs-mysql:
	docker compose logs -f mysql
 
logs-rabbitmq:
	docker compose logs -f rabbitmq
 	
ps:
	docker compose ps

start-api:
	docker compose up -d api

start-worker:
	docker compose up -d worker
        
start-deps:
	docker compose up -d rabbitmq mysql

restart-api:
	docker compose restart api
    
restart-worker:
	docker compose restart worker

stop-deps:
	docker compose stop rabbitmq mysql