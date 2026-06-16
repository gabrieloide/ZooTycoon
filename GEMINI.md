# 🤖 GEMINI AI CONTEXT & RULES

## 📜 Reglas de Código (Inquebrantables)
- **Estricto Inglés:** Toda la nomenclatura de Variables, Clases, Funciones y Archivos debe ser **única y exclusivamente en inglés**.
- **Cero Comentarios:** No escribir comentarios (`//`) ni documentaciones XML (`///`) en el código fuente generado. El código debe ser completamente limpio y autoexplicativo por defecto, utilizando convenciones claras (`PascalCase` y `camelCase`).

## 🤝 División del Trabajo (Acuerdo)
* **Gemini (IA):** Se encarga del código "tedioso" y boilerplate (estructuras de datos complejas, conectividad de UI, sistemas de persistencia, gestores de tiempo, lógicas matemáticas complejas y diccionarios O(1)).
* **Gabriel (Desarrollador):** Se encarga del "core", el "game feel" (interacción física, construcción, sensaciones visuales/sonoras e integraciones finales en el Editor).

---

# 📜 GDD: Panic Zoo (Core Prototype)

## 1. Pitch del Prototipo
Un simulador de gestión económica y espacial donde el jugador administra un zoológico de fantasía bajo un modelo de **concesiones y licencias**. El objetivo es maximizar la rentabilidad en turnos de **15 minutos**, equilibrando la expansión de biomas exóticos (Nubes, Volcanes, etc.) con una **matriz de compatibilidad animal** estricta. El éxito depende de la planificación financiera y la mitigación de desastres: cada decisión de construcción afecta el estrés de los animales, lo que puede derivar en pérdidas masivas de capital al cierre de caja.

## 2. Ficha Rápida
- **Fantasía del jugador:** Gerente de operaciones de un zoo de alto riesgo.
- **Género:** Business Tycoon / Puzzle de gestión.
- **Objetivo Principal:** Sobrevivir al ciclo de 15 minutos con saldo positivo y licencias desbloqueadas.
- **Conflicto Clave:** Espacio vs. Compatibilidad. (¿Pongo este animal aquí para ganar dinero rápido aunque estrese al vecino?).

## 3. Core Loop (Ciclo de Juego)
1. **Firma de Contratos:** El jugador accede a la "Oficina" para comprar licencias de uso de biomas y objetos.
2. **Construcción (Drag & Drop):** Se crean los hábitats y se colocan decoraciones/servicios. El coste se descuenta al instante.
3. **Operación y Spawning:** Se posicionan animales que generan ingresos pasivos por segundo.
4. **Cálculo de Tensión:** El sistema monitoriza en tiempo real la **Matrix de Compatibilidad** basada en:
    - Tipo de Bioma (¿Es el correcto?).
    - Compañeros de recinto (¿Se llevan bien?).
    - Vecindad (¿El hábitat contiguo le molesta?).
5. **Liquidación de Turno:** Al minuto 15, se realiza el balance: `Ingresos - (Mantenimiento + Multas por Desastres)`.

## 4. Mecánicas Principales
### A. Sistema de Hábitats y Biomas
- **Reconocimiento de Área:** Cada hábitat creado mediante drag-and-drop genera una zona lógica con un ID único y un tipo de bioma definido (Volcán, Nube, Tundra, etc.).
- **Validación Espacial:** Sistema de celdas para evitar solapamientos y delimitar fronteras.

### B. Lógica de Estrés y Desastre
- **Matriz de Compatibilidad:** Una base de datos que define la relación entre familias de animales.
- **Medidor de Estrés:** Barra acumulativa que sube por incompatibilidad.
- **Evento de Desastre:** Al llegar al 100% de estrés, ocurre un "Desastre" que genera una penalización económica directa (multa/reparación) y reduce el ingreso del animal.

### C. Economía de Licencias
- **Contratos de Uso:** En lugar de comprar objetos uno a uno en una tienda abierta, el jugador debe "firmar" licencias que habilitan categorías de construcción.
- **Gasto Directo:** Una vez firmada la licencia, colocar cada objeto tiene un coste de construcción individual que se resta del saldo global.

## 5. El Ciclo de Tiempo (El Turno)
- **Duración:** 15 minutos reales.
- **Urgencia:** El tiempo no se detiene. El estrés de los animales escala según avanza el reloj, obligando a usar **Acciones de Mitigación** o reubicar animales antes del cierre de caja.

## 6. Progresión del Prototipo
- **Económica:** Inicias con capital bajo y licencias básicas (Pasto).
- **Desbloqueo:** El dinero acumulado en ciclos anteriores permite firmar contratos más caros (Volcán/Nube) que permiten animales con mayores ingresos pero mayor riesgo de conflicto.
