# 📌 Guía para ejecutar los experimentos de carga con Docker Compose y K6

Este documento detalla paso a paso cómo levantar los servicios y ejecutar las pruebas de resiliencia usando `k6`.

---

## 📂 Estructura del Proyecto

```bash
ProyectoIntegrador1/
│── experimento-hu021/
│   ├── docker-compose.yaml
│   ├── .env
│   ├── Pruebas/
│   │   ├── prueba_resiliencia_pedidos_5m.js
│   │   ├── prueba_resiliencia_pedidos_10m.js
│   │   ├── prueba_resiliencia_pedidos_15m.js
```

---

## 🔹 Descripción del Experimento

Los experimentos evalúan la **resiliencia del sistema** ante diferentes cargas de solicitudes al servicio de **pedidos** (`http://localhost:8080/pedidos/`).

Cada prueba simula **usuarios generando pedidos con productos aleatorios**, incrementando y reduciendo gradualmente la carga en el sistema.

### **Objetivos del experimento:**
✔ Medir el **tiempo de respuesta** (`http_req_duration`, 99% de las solicitudes deben ser <500ms).  
✔ Evaluar la **tasa de errores** (`http_req_failed`, no debe superar el 1%).  
✔ Determinar el **comportamiento del sistema bajo carga sostenida**.

---

## 📌 Resumen de los Scripts de Prueba

| Script | Duración Total | Carga Inicial | Carga Máxima | Carga Final | Objetivo |
|--------|---------------|--------------|--------------|-------------|----------|
| **prueba_resiliencia_pedidos_5m.js** | **5 minutos** | 100 usuarios | 200 usuarios | 0 usuarios | Evaluar desempeño en carga moderada a corto plazo |
| **prueba_resiliencia_pedidos_10m.js** | **10 minutos** | 100 usuarios | 200 usuarios | 0 usuarios | Evaluar comportamiento con carga sostenida más tiempo |
| **prueba_resiliencia_pedidos_15m.js** | **15 minutos** | 100 usuarios | 200 usuarios | 0 usuarios | Medir degradación del servicio en cargas prolongadas |

---

## 🔌 Puertos expuestos y cómo conectarse

Los siguientes servicios están disponibles desde el **host**:

| Servicio        | Puerto en Docker | Conexión desde Host |
|----------------|----------------|--------------------|
| **Ventas API** | `8080:8080`     | `http://localhost:8080` |
| **PostgreSQL** | `5432:5432`     | `postgres://postgres:postgres@localhost:5432/ventas` |
| **Redis**      | `8001:8001`      | `http://localhost:8001` |
| **Kibana**     | `5601:5601`      | `http://localhost:5601`  (usuario: elastic, contraseña: pass123)|
| **RabbitMQ UI** | `15672:15672`   | `http://localhost:15672` (usuario: `guest`, contraseña: `guest`) |

📌 **Importante**: Otros servicios como **Elasticsearch y APM Server** solo son accesibles dentro de los contenedores en la red interna de Docker.

---

## 🚀 PASO 1: Levantar los servicios con Docker Compose

Abre una terminal en la carpeta `experimento-hu021/` y ejecuta:

```bash
docker compose up -d
```

---

## 🏋️ PASO 2: Ejecutar los experimentos de carga con K6

Ejecuta cada prueba en **una terminal abierta en la carpeta `experimento-hu021/`**.

### 📌 Ejecutar la prueba de resiliencia de 5 minutos

```bash
docker run --rm -v "${PWD}/Pruebas:/scripts" -w /scripts grafana/k6 run prueba_resiliencia_pedidos_5m.js --out json=prueba_resiliencia_pedidos_5m.json
```

✔ **Genera el archivo:** `prueba_resiliencia_pedidos_5m.json`  

📌 **Cuando termine la prueba, procede al PASO 3 antes de ejecutar otra prueba**  

---

### 📌 Ejecutar la prueba de resiliencia de 10 minutos

1️⃣ Primero, limpia los servicios antes de comenzar la siguiente prueba:
```bash
docker compose down
docker compose up -d
```

2️⃣ Luego, ejecuta la prueba en la **misma terminal**:
```bash
docker run --rm -v "${PWD}/Pruebas:/scripts" -w /scripts grafana/k6 run prueba_resiliencia_pedidos_10m.js --out json=prueba_resiliencia_pedidos_10m.json
```

✔ **Genera el archivo:** `prueba_resiliencia_pedidos_10m.json`  

📌 **Cuando termine la prueba, procede al PASO 3 antes de ejecutar otra prueba**  

---

### 📌 Ejecutar la prueba de resiliencia de 15 minutos

1️⃣ Primero, limpia los servicios antes de comenzar la siguiente prueba:
```bash
docker compose down
docker compose up -d
```

2️⃣ Luego, ejecuta la prueba en la **misma terminal**:
```bash
docker run --rm -v "${PWD}/Pruebas:/scripts" -w /scripts grafana/k6 run prueba_resiliencia_pedidos_15m.js --out json=prueba_resiliencia_pedidos_15m.json
```

✔ **Genera el archivo:** `prueba_resiliencia_pedidos_15m.json`  

---

## 🛑 PASO 3: Limpiar los contenedores después de cada prueba

Después de cada ejecución, **es obligatorio detener los contenedores** para limpiar el entorno antes de la siguiente prueba.

```bash
docker compose down
```

---

## 📊 PASO 4: Revisar los reportes de resultados

Cada prueba genera **un archivo de salida en formato JSON** y **un reporte HTML**.

Para visualizar los reportes HTML, **abre cada archivo con tu navegador**.

---

## 🔍 Solución de problemas

🔹 **¿Los servicios no levantan?**
```bash
docker compose logs -f
```

🔹 **¿Error de conexión en K6?**
- Asegúrate de que la API está disponible en `http://localhost:8080/pedidos/`
- Verifica que `docker compose up -d` se ejecutó correctamente.

🔹 **¿Quedan contenedores en ejecución después de una prueba?**
```bash
docker ps
docker compose down
```

---

### 🚀 Con esto ya puedes ejecutar los experimentos de resiliencia ¡Buena suerte! 🚀
