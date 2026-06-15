# 🤖 AI CONTEXT & RULES

## 📜 Reglas de Código (Inquebrantables)
- **Estricto Inglés:** Toda la nomenclatura de Variables, Clases, Funciones y Archivos debe ser **única y exclusivamente en inglés**.
- **Cero Comentarios:** No escribir comentarios (`//`) ni documentaciones XML (`///`) en el código fuente generado. El código debe ser completamente limpio y autoexplicativo por defecto, utilizando convenciones claras (`PascalCase` y `camelCase`).
- **Datos en ScriptableObjects:** TODO valor de balanceo (ingresos, costos, velocidad de estrés, estamina, frecuencia de eventos) vive en ScriptableObjects o campos expuestos en Inspector. Nunca números mágicos hardcodeados — el tuning se itera sin recompilar.

## 🤝 División del Trabajo (Acuerdo)
* **IA:** Se encarga del código "tedioso" y boilerplate (estructuras de datos complejas, conectividad de UI, sistemas de persistencia, gestores de tiempo, lógicas matemáticas complejas y diccionarios O(1)).
* **Gabriel (Desarrollador):** Se encarga del "core", el "game feel" (interacción física, construcción, sensaciones visuales/sonoras e integraciones finales en el Editor).

---

# 📜 GDD: Panic Zoo (Core Prototype)

## 1. Pitch del Prototipo
Un juego de gestión de crisis física donde el jugador controla en persona al gerente de un zoológico de fantasía bajo un modelo de **concesiones y licencias**. **Ciclo de día/noche estilo Stardew Valley** (duración de día configurable): de día (8:00–18:00) el zoo abre y los visitantes pagan taquilla; de noche está cerrado y construyes. El ingreso entra por visitantes satisfechos; los animales estresados se escapan y los espantan. Liquidación de caja al cierre de cada día. El éxito depende de la planificación espacial y de cuántas crisis puedes atender físicamente con estamina limitada.

**Referencia:** Overcooked (caos físico) + Theme Hospital (gestión). **Feeling:** Urgencia, Caos, Consecuencias.

## 2. Ficha Rápida
- **Fantasía del jugador:** Gerente de operaciones de un zoo de alto riesgo — en persona, no desde un menú.
- **Género:** Crisis Management / Tycoon físico.
- **Objetivo Principal:** Sobrevivir sin quebrar (demo: 3 días con escalada — día 1 Pasto, día 2 Desierto, día 3 Nieve). El dinero final es el score.
- **Conflicto Clave:** Espacio vs. Compatibilidad vs. Distancia. (¿Pongo este animal rentable aquí aunque estrese al vecino Y quede lejos para atenderlo?).

## 3. Core Loop (Ciclo de Juego)
1. **Noche (zoo cerrado):** Firmar licencias en La Oficina y construir sin presión. La distribución del zoo es el diseño de tu propio nivel para el día siguiente.
2. **Construcción física (Drag & Drop):** El gerente se desplaza físicamente para construir. El coste se descuenta al instante.
3. **Día (apertura 8:00–18:00):** Entran visitantes que pagan taquilla. Animales estresados o escapados espantan visitantes (huyen sin pagar). Durante el día no se construye, solo se mitiga.
4. **Cálculo de Tensión:** El sistema monitoriza en tiempo real la **Matriz de Compatibilidad** basada en:
    - Tipo de Bioma (¿Es el correcto?).
    - Compañeros de recinto (¿Se llevan bien?).
    - Vecindad (¿El hábitat contiguo le molesta?).
5. **Liquidación Diaria:** Al cierre (18:00) de cada día: `Ingresos por visitantes - (Construcción + Mantenimiento + Pérdidas por desastres)`. Si cierras en negativo, La Oficina ofrece un **préstamo de emergencia con interés diario**; quiebra real solo si no puedes cubrirlo. Perder debe ser difícil, no un precipicio.

## 4. Mecánicas Principales
### A. Sistema de Hábitats y Biomas
- **Reconocimiento de Área:** Cada hábitat drag-and-drop genera una zona lógica con ID único y bioma definido (demo: Pasto, Desierto, Nieve; expansión futura: Volcán, Nube, etc.).
- **Validación Espacial:** Sistema de celdas para evitar solapamientos y delimitar fronteras.

### B. Lógica de Estrés y Desastre
- **Matriz de Compatibilidad:** Base de datos que define la relación entre familias de animales.
- **Medidor de Estrés:** Barra acumulativa por incompatibilidad. Feedback visual: caritas 2D billboard (verde → amarillo → rojo).
- **Evento de Desastre (Escape):** Al 100% de estrés el animal **rompe la valla y se escapa**, espantando visitantes hasta que el gerente lo recaptura físicamente. Pérdida económica emergente (visitantes perdidos + reparación in situ), no multa abstracta.

### C. Economía de Licencias y Visitantes
- **Contratos de Uso:** Licencias que habilitan categorías de construcción (La Oficina es SOLO para esto).
- **Gasto Directo:** Colocar cada objeto tiene coste individual que se resta del saldo.
- **Ingreso por Visitante:** Taquilla + satisfacción. Sin visitantes felices no hay ingreso.
- **Préstamo de Emergencia:** Si un día cierra en negativo, se ofrece firmar un préstamo (estética de contrato) con interés diario. Día malo = remontada, no game over abrupto.

### D. El Gerente (Avatar Físico) — YA IMPLEMENTADO (base)
- **Control directo:** Personaje con estamina que se desplaza físicamente para construir, mitigar y recapturar.
- **Estamina:** Limita cuántas crisis se atienden por minuto. Junto con la distancia, es el dial central de balanceo (frecuencia de eventos vs. capacidad de movimiento — el dial de Overcooked).

## 5. El Ciclo de Tiempo (Día/Noche)
- **Duración:** Cada día dura X minutos reales (configurable en TimeManager, implementado con evento onDayChanged).
- **Estructura del día:**
  - **6:00** — Comienza el día. Zoo cerrado, sin visitantes. Ventana de planificación y construcción antes del caos.
  - **8:00** — Abren las puertas. Visitantes entran y pagan taquilla. El ingreso empieza a subir. El estrés de animales tiene consecuencias económicas reales.
  - **18:00** — Cierran las puertas. Visitantes salen, el ingreso se detiene. El jugador puede seguir moviéndose y construyendo, pero no hay más actividad económica hasta el siguiente día.
  - **Dormir (trigger manual)** — El jugador decide cuándo terminar el día yendo a dormir. Eso dispara la liquidación diaria y avanza al día siguiente (6:00).
- **Build mode:** SIEMPRE disponible, en cualquier momento del día. No se bloquea al abrir el zoo.
- **Urgencia:** El estrés escala durante las horas de apertura (8:00–18:00) y la estamina limita cuántas crisis puedes atender antes del cierre.

## 6. Progresión del Prototipo
- **Económica:** Capital bajo inicial y licencia básica (Pasto).
- **Desbloqueo:** El dinero acumulado día a día permite firmar contratos más caros (Desierto/Nieve en la demo; biomas exóticos como Volcán o Nube en el juego completo): animales más rentables, mayor riesgo, y más terreno que cubrir a pie.

## 7. Estado Actual (15 jun 2026)
- ✅ Grid System O(1) · Habitat Builder con validación holográfica · Cámaras Cinemachine 3 · Avatar con estamina (sprint/drain/regen/tired) · TimeManager con ciclo de días y horario 8:00–18:00 · CompatibilityMatrix (SO) + acumulación de estrés en Animal · AnimalData completo · Shop con 4 tabs (Habitats/Animals/Decorations/Licenses) · HUD parcial + StaminaUI · `BiomeDefinition.description` + `LicenseData.description` (campos TextArea) · Tooltip de descripción en hover de items del shop · Tab de Licencias en BuildPanel.uxml · `ShopDetector.IsOverShop` estático bloqueando raycast y gizmos vía UI Toolkit TrickleDown · Precios de biomas actualizados a modelo por tile en Notion (Grassland $100 · Desert $150 · Snow $200 · Rivers $120 · Jungle $130; mantenimiento diario por tile también).
- ⏳ Pendiente: cobro real por tile al construir (lógica en HabitatBuilder.FinalizeBuild) · EconomyManager conectado a construcción y taquilla · estado día/noche en gameplay (puertas con onDayChanged) · completar Stress AI (bioma + sobrepoblación + caritas 2D) · escape/recaptura · firma de licencias (lógica, no solo UI) · liquidación diaria.
- 🔧 Fixes pendientes: quitar `readonly` de campos serializados en TimeManager · mover números mágicos de Animal.cs (0.05f, 1f) a ScriptableObjects · recalcular tensión por eventos, no cada frame · asignar en Inspector: `availableLicenses` en GameHUDController + `description` en los 3 assets de licencia.
- 📅 Demo en Itch.io: 16 jul 2026. Checkpoint de diversión: fin de semana 3 (2 jul).