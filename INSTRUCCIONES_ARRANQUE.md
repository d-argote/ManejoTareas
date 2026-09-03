# ManejoTareas – Instrucciones de Arranque

> Problema que tenías: al apagar el equipo, `docker.service` quedaba `inactive (dead)` y `PostgreSQL` en `localhost:5432` no respondía → `Program.cs:19 db.Database.Migrate()` lanzaba `NpgsqlException: Connection refused` y la web crasheaba.

Este manual es para que **no tengas que hacerlo todo de nuevo** cada reinicio.

---

## 1. Configuración ÚNICA (solo una vez)

Ejecuta esto **una sola vez** en terminal:

```bash
# Habilitar Docker para que arranque solo al encender el equipo
sudo systemctl enable --now docker
sudo systemctl enable docker.socket

# Dar permisos a tu usuario para usar docker sin sudo (requiere cerrar sesión después)
sudo usermod -aG docker $USER

# Levantar Postgres con reinicio automático (usa el docker-compose.yml del proyecto)
cd /home/mrx/Desktop/ManejoTareas
sudo docker compose up -d
# Verifica:
sudo docker ps
pg_isready -h localhost -p 5432
```

> `docker-compose.yml:10` tiene `restart: unless-stopped`, por eso con `systemctl enable docker` el contenedor `manejotareas-postgres` volverá solo después de reiniciar.

**Importante:** después de `usermod` cierra sesión y vuelve a entrar para que `groups` muestre `docker`.

Ya está arreglado también `Program.cs:16-27` (try/catch en `Migrate`) y `mise` con `dotnet 9.0.317 + 10.0.400` para que no crashee aunque la DB esté caída.

---

## 2. Uso Diario (después de encender el equipo)

Ya no necesitas repetir todo. Solo:

```bash
cd /home/mrx/Desktop/ManejoTareas

# Opcional: verificar que todo levantó solo
sudo docker ps | grep postgres
pg_isready -h localhost -p 5432  # debe decir: accepting connections

# Si por algo no levantó (ej. Docker no inició):
sudo systemctl start docker
sudo docker compose up -d

# Iniciar la web
dotnet run --urls "http://localhost:5293"
# Alternativa si mise falla: /usr/share/dotnet/dotnet run --urls "http://localhost:5293"
```

Abre en navegador: `http://localhost:5293`

Detener:
```bash
Ctrl+C
# No hagas `docker compose down` a menos que quieras borrar el contenedor
# (los datos se guardan en el volumen pgdata)
```

---

## 3. Verificación Rápida

```bash
systemctl is-active docker          # debe ser: active
sudo docker ps                      # debe mostrar manejotareas-postgres Up
PGPASSWORD=postgres psql -h localhost -U postgres -d manejotareas -c "SELECT 1"
curl -I http://localhost:5293/      # debe ser 200
```

---

## 4. Si Algo Falla

| Síntoma | Causa | Solución |
|---|---|---|
| `permission denied /var/run/docker.sock` | No estás en grupo `docker` | `newgrp docker` o cerrar sesión |
| `localhost:5432 - sin respuesta` | Docker apagado | `sudo systemctl start docker && sudo docker compose up -d` |
| `Failed to connect to 127.0.0.1:5432` en `Program.cs:21` | Postgres no levantó | `sudo docker logs manejotareas-postgres` |
| `Framework 9.0.0 not found` | `mise` sin runtime 9 | `mise install dotnet@9.0` (ya hecho) |
| `Failed to determine https port` | Aviso de `UseHttpsRedirection` en `http` | Ignorable en desarrollo |

---

## 5. Archivos Clave

- `Program.cs:21` - `db.Database.Migrate()` ahora con try/catch, no tumba la app
- `appsettings.json:11` - `Host=localhost;Port=5432;Database=manejotareas;Username=postgres;Password=postgres`
- `docker-compose.yml:1` - definición de Postgres con `restart: unless-stopped`
- `ManejoTareas.csproj:4` - `net9.0` (requiere runtime 9)
