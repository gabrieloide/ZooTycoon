# Panic Zoo - Itch.io Demo Scope

## 🎯 Objetivo de la Demo
Publicar una versión jugable (MVP) en Itch.io de **"Panic Zoo"**: gestión de crisis física con **ciclo día/noche estilo Stardew** (día configurable, ~5-7 min). El jugador controla al gerente en persona: de noche construye, de día apaga incendios con el público dentro. **Demo: sobrevivir 3 días** con escalada de dificultad (día 1 Pasto → día 2 Desierto → día 3 Nieve). El dinero final es el score. Biomas exóticos (Volcán, Nube, etc.) quedan para el juego completo.

**Timeline:** 5 semanas (12 jun → 16 jul 2026). Checkpoint de validación de diversión al final de la semana 3.

## 🕹️ Core Loop de la Demo (Ciclo Día/Noche)
1. **Noche (zoo cerrado):** El jugador firma licencias en La Oficina y construye recintos sin presión. La distribución del zoo es diseño de su propio nivel: las distancias determinan cuánto tardará en atender crisis al día siguiente.
2. **Construcción física:** El gerente (avatar con estamina) se desplaza físicamente para construir y poblar. El coste se descuenta al instante.
3. **Día (apertura 8:00–18:00):** Las puertas abren y los visitantes pagan taquilla. El ingreso depende de visitantes satisfechos: animales estresados o escapados los espantan (huyen sin pagar). Durante el día no se construye — solo se mitiga.
4. **Escalar el Riesgo:** Para cubrir el mantenimiento hay que firmar licencias más caras (Desierto, Nieve) con animales más rentables pero más conflictivos, en una cuadrícula cada vez más apretada.
5. **Desastre (Escape físico):** Si el estrés llega a 100%, el animal **rompe la valla y se escapa**, espantando visitantes hasta que el gerente lo persigue y recaptura (gasta estamina). La valla se repara in situ, no desde menú. La pérdida económica es emergente: visitantes perdidos + reparación.
6. **Liquidación Diaria (18:00):** `(Ingresos por visitantes) - (Construcción + Mantenimiento + Pérdidas por desastres)`. Si cierras en negativo, La Oficina ofrece un **préstamo de emergencia con interés diario**; quiebra real solo si no puedes cubrirlo. Sobrevive los 3 días = victoria, dinero final = score.

## 📦 Contenido y Mecánicas del MVP
*   **Reloj Día/Noche:** Hora del día + número de día en HUD (TimeManager ya implementado: horario 8:00–18:00, evento onDayChanged). Duración del día configurable.
*   **Ciclo Día/Noche:** Día = zoo abierto (caos, mitigación física). Noche = cerrado (construcción). Dos diales de dificultad independientes.
*   **Préstamo de Emergencia:** Cierre en negativo → contrato de préstamo con interés diario. Perder debe ser difícil; el día malo es remontada, no precipicio.
*   **El Gerente (Avatar):** Control directo con estamina. Moverse, construir, recapturar y reparar cuestan energía. Estamina + distancia = dial central de balanceo (frecuencia de eventos vs. capacidad de respuesta, estilo Overcooked).
*   **Sistema de Licencias (UI):** Menú de La Oficina con estética de contratos. La Oficina es SOLO para licencias — toda mitigación es física.
*   **Economía por Visitante:** Taquilla + satisfacción. Visitantes asustados huyen sin pagar.
*   **Matriz de Compatibilidad:**
    *   **3 Biomas Básicos:** Pasto (inicial), Desierto (intermedio), Nieve (avanzado). Exóticos (Volcán, Nube) = juego completo.
    *   **3 Especies Base** con compatibilidades cruzadas (ej: el animal de Nieve se estresa si su recinto toca Desierto).
*   **Castigo Económico (Desastres):** Sin game-over inmediato. El jugador decide en caliente: ¿persigo al animal escapado o sigo construyendo y asumo la sangría de visitantes?

## 🎯 Checkpoint de Validación (fin Semana 3)
3–5 personas externas juegan el loop completo con 1 bioma. Si piden otra ronda → la diversión está validada. Si no → se ajusta el loop ANTES de invertir en UI y contenido.

---

## 🎨 Lista de Assets a Desarrollar (Para Gabriel)

### 🐾 Animales (3 Especies Temáticas)
*   [ ] **Especie de Pasto (ej: Oveja/Vaca):** Modelo 3D low-poly + Animaciones (Idle, Caminar, Pánico/Escape).
*   [ ] **Especie de Desierto (ej: Camello/Zorro):** Modelo 3D + Animaciones.
*   [ ] **Especie de Nieve (ej: Pingüino/Oso polar):** Modelo 3D + Animaciones.
*   [ ] **Caritas de estrés 2D (billboard):** 3 estados (verde/amarillo/rojo) legibles a distancia. Reemplazan expresiones modeladas — NO se hacen modelos hiperrealistas.

### 🧑‍💼 El Gerente
*   [ ] Modelo simple + Animaciones (Idle, Correr, Construir, Recapturar).
*   [ ] VFX/feedback de estamina agotada.

### 🏗️ Construcción, Biomas y Entorno
*   [ ] **Texturas / Tiles de Suelo (3 Biomas):** Pasto, Arena/Desierto, Nieve/Hielo.
*   [ ] **Vallas:** Reja Básica (+ *Valla Rota* — crucial para el desastre).
*   [ ] **Puertas del Zoo:** Entrada visible que marque el ciclo cerrado/abierto.
*   [ ] **Caminos de Visitantes:** Tile de camino.
*   [ ] **Prop "La Oficina":** Escritorio con PC y papeles (punto de interacción para licencias).

### 👥 Visitantes (NPCs)
*   [ ] **Visitante Genérico:** Modelo 3D sencillo.
    *   *Animaciones:* Caminar observando (tranquilo), Correr (huyendo en pánico).
    *   *Feedback:* indicador de pago en taquilla (moneda flotante o similar).

### 🖥️ Interfaz de Usuario (UI)
*   [ ] **Estética de Contratos:** UI de firma de documentos corporativos para La Oficina.
*   [ ] **HUD de Urgencia:** Timer llamativo + barra de estamina + contador de visitantes/ingresos.
*   [ ] **Pantalla de Liquidación:** desglose completo del balance.