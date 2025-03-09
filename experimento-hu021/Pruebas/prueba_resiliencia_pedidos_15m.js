import http from 'k6/http';
import { check, sleep } from 'k6';
import { htmlReport } from "https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js";

export let options = {
  stages: [
    { duration: '2m', target: 100 },
    { duration: '11m', target: 200 },
    { duration: '2m', target: 0 },
  ],
  thresholds: {
    'http_req_duration': ['p(99)<500'], // El 99% de las solicitudes deben tardar menos de 500ms
    'http_req_failed': ['rate<0.01'], // Máximo 1% de fallos en comunicación
  },
};

function getRandomClienteId() {
  return Math.floor(Math.random() * 10000) + 1;
}

function getRandomProductos() {
  const productosDisponibles = [
    { nombre: 'Arroz 1kg', cantidad: 1 },
    { nombre: 'Aceite 1L', cantidad: 1 },
    { nombre: 'Harina 1kg', cantidad: 2 },
    { nombre: 'Azúcar 1kg', cantidad: 3 },
    { nombre: 'Sal 500g', cantidad: 1 },
  ];

  let numProductos = Math.floor(Math.random() * 3) + 1; // Entre 1 y 3 productos
  return productosDisponibles.sort(() => 0.5 - Math.random()).slice(0, numProductos);
}

export default function () {
  const url = 'http://host.docker.internal:8080/pedidos/'; 
  const payload = JSON.stringify({
    clienteId: getRandomClienteId(),
    productos: getRandomProductos(),
  });

  const params = {
    headers: { 'Content-Type': 'application/json' },
  };

  let res = http.post(url, payload, params);

  check(res, {
    'El endpoint responde correctamente con 200': (r) => r.status === 200,
  });

  sleep(1); // Pausa de 1s entre solicitudes para mantener el ritmo
}

export function handleSummary(data) {
  return {
    "prueba_resiliencia_pedidos_15m.html": htmlReport(data),
  };
}
