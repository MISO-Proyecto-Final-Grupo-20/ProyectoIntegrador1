import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
  stages: [
    { duration: '30s', target: 20 }, // Rampa de subida
    { duration: '5m', target: 20 },  // Carga sostenida
    { duration: '30s', target: 0 },  // Rampa de bajada
  ],
  thresholds: {
    http_req_duration: ['p(99)<1000'], // 95% de las solicitudes deben responder en <1s
  },
};

export default function () {
  const url = 'http://host.docker.internal:5000/pedidos/'; // Ajusta la URL según tu entorno
  const payload = JSON.stringify({
    clienteId: Math.floor(Math.random() * 10000),
    productos: [
      { nombre: 'Arroz 1kg', cantidad: Math.floor(Math.random() * 5) + 1 },
      { nombre: 'Aceite 1L', cantidad: Math.floor(Math.random() * 3) + 1 },
    ],
  });

  const params = { headers: { 'Content-Type': 'application/json' } };
  const res = http.post(url, payload, params);

  check(res, {
    'El endpoint responde con 200': (r) => r.status === 200,
    'El endpoint responde en menos de 1s': (r) => r.timings.duration < 1000,
  });

  sleep(1);
}