# 📌 Guía para ejecutar los experimentos de tiempo de respuesta con Docker Compose y K6

Este documento detalla paso a paso cómo levantar los servicios y ejecutar las pruebas de tiempo de respuesta usando `k6`.

---

## 📂 Estructura del Proyecto

```bash
experimento-hu012-ha10/
│── docker-compose.yaml
│── .env
│── Pruebas/
│   ├── prueba_tiempo_respuesta_5m.js
│   ├── prueba_tiempo_respuesta_10m.js
│   ├── prueba_tiempo_respuesta_15m.js
│   ├── prueba_tiempo_respuesta_5m.json
│   ├── prueba_tiempo_respuesta_10m.json
│   ├── prueba_tiempo_respuesta_15m.json
│   ├── prueba_tiempo_respuesta_5m.html
│   ├── prueba_tiempo_respuesta_10m.html
│   ├── prueba_tiempo_respuesta_15m.html
```

---

## 🔹 Descripción del Experimento

Este experimento evalúa el **tiempo de respuesta del servicio de pedidos** (`http://host.docker.internal:8090/pedidos/`).

Cada prueba mide el tiempo de respuesta del endpoint bajo carga sostenida en diferentes períodos de tiempo.

### **Objetivos del experimento:**
✔ Validar que el tiempo de respuesta sea **inferior a 1 segundo en el 99% de los casos**.  
✔ Medir la latencia (`http_req_duration`).  
✔ Evaluar el comportamiento del sistema con cargas de **5, 10 y 15 minutos**.

---

## 📌 Resumen de los Scripts de Prueba

| Script | Duración Total | Carga Inicial | Carga Máxima | Carga Final | Objetivo |
|--------|---------------|--------------|--------------|-------------|----------|
| **prueba_tiempo_respuesta_5m.js** | **5 minutos** | 100 usuarios | 100 usuarios | 0 usuarios | Evaluar desempeño en carga moderada |
| **prueba_tiempo_respuesta_10m.js** | **10 minutos** | 100 usuarios | 100 usuarios | 0 usuarios | Evaluar comportamiento con carga sostenida |
| **prueba_tiempo_respuesta_15m.js** | **15 minutos** | 100 usuarios | 100 usuarios | 0 usuarios | Medir degradación del servicio en cargas prolongadas |

---

## 🔌 Puertos expuestos y cómo conectarse

Los siguientes servicios están disponibles desde el **host**:

| Servicio        | Puerto en Docker | Conexión desde Host |
|----------------|----------------|--------------------|
| **Ventas API** | `8090:8080`     | `http://host.docker.internal:8090/pedidos/` |
| **PostgreSQL** | `5433:5432`     | `postgres://postgres:postgres@localhost:5433/ventas` |
| **RabbitMQ UI** | `15673:15672`   | `http://localhost:15673` (usuario: `guest`, contraseña: `guest`) |
| **Kibana**     | `5602:5601`      | `http://localhost:5602` (usuario: `elastic`, contraseña: `pass123`) |

📌 **Importante**: Otros servicios solo son accesibles dentro de los contenedores en la red interna de Docker.

---

## 🚀 PASO 1: Levantar los servicios con Docker Compose

Abre una terminal en la carpeta `experimento-hu012-ha10/` y ejecuta:

```bash
docker compose up -d
```

---

## 🏋️ PASO 2: Ejecutar los experimentos de carga con K6

Ejecuta cada prueba en **una terminal abierta en la carpeta `experimento-hu012-ha10/`**.

### 📌 Ejecutar la prueba de tiempo de respuesta de 5 minutos

```bash
docker run --rm -v "${PWD}/Pruebas:/scripts" -w /scripts grafana/k6 run prueba_tiempo_respuesta_5m.js --out json=prueba_tiempo_respuesta_5m.json
```

✔ **Genera los archivos:** `prueba_tiempo_respuesta_5m.json`, `prueba_tiempo_respuesta_5m.html`  

📌 **Cuando termine la prueba, procede al PASO 3 antes de ejecutar otra prueba**  

---

### 📌 Ejecutar la prueba de tiempo de respuesta de 10 minutos

1️⃣ Primero, limpia los servicios antes de comenzar la siguiente prueba:
```bash
docker compose down
docker compose up -d
```

2️⃣ Luego, ejecuta la prueba en la **misma terminal**:
```bash
docker run --rm -v "${PWD}/Pruebas:/scripts" -w /scripts grafana/k6 run prueba_tiempo_respuesta_10m.js --out json=prueba_tiempo_respuesta_10m.json
```

✔ **Genera los archivos:** `prueba_tiempo_respuesta_10m.json`, `prueba_tiempo_respuesta_10m.html`  

📌 **Cuando termine la prueba, procede al PASO 3 antes de ejecutar otra prueba**  

---

### 📌 Ejecutar la prueba de tiempo de respuesta de 15 minutos

1️⃣ Primero, limpia los servicios antes de comenzar la siguiente prueba:
```bash
docker compose down
docker compose up -d
```

2️⃣ Luego, ejecuta la prueba en la **misma terminal**:
```bash
docker run --rm -v "${PWD}/Pruebas:/scripts" -w /scripts grafana/k6 run prueba_tiempo_respuesta_15m.js --out json=prueba_tiempo_respuesta_15m.json
```

✔ **Genera los archivos:** `prueba_tiempo_respuesta_15m.json`, `prueba_tiempo_respuesta_15m.html`  

---

## 🛑 PASO 3: Limpiar los contenedores después de cada prueba

Después de cada ejecución, **es obligatorio detener los contenedores** para limpiar el entorno antes de la siguiente prueba.

```bash
docker compose down
```

---

## 📊 PASO 4: Revisar los reportes de resultados

Cada prueba genera **un archivo de salida en formato JSON** y **un reporte HTML**.

📌 **Para ver los resultados, abra los archivos HTML en su navegador.**

---

## 🔍 Solución de problemas

🔹 **¿Los servicios no levantan?**
```bash
docker compose logs -f
```

🔹 **¿Error de conexión en K6?**
- Asegúrate de que la API está disponible en `http://host.docker.internal:8090/pedidos/`
- Verifica que `docker compose up -d` se ejecutó correctamente.

🔹 **¿Quedan contenedores en ejecución después de una prueba?**
```bash
docker ps
docker compose down
```

---

### 🚀 Con esto ya puedes ejecutar los experimentos de tiempo de respuesta ¡Buena suerte! 🚀
