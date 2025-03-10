import http from 'k6/http';
import { check, sleep } from 'k6';
import { htmlReport } from "https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js";

export let options = {
  stages: [
    { duration: '30s', target: 100 }, // Rampa de subida
    { duration: '9m', target: 100 },  // Carga sostenida
    { duration: '30s', target: 0 },  // Rampa de bajada
  ],
  thresholds: {
    http_req_duration: ['p(99)<1000'], // 99% de las solicitudes deben responder en <1s
  },
};

export default function () {
  const url = 'http://host.docker.internal:8090/pedidos/'; // Ajusta la URL según tu entorno
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

export function handleSummary(data) {
  return {
    "prueba_tiempo_respuesta_10m.html": htmlReport(data),
  };
}