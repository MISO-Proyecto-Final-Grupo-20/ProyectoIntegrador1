para correr las pruebas 
docker run --rm -v "${PWD}/pruebas:/scripts" -w /scripts grafana/k6 run prueba_resiliencia_pedidos.js --out json=test.json 