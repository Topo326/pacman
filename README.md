# Pac-Man Capstone Project

Este proyecto es una implementación moderna del clásico juego Arcade **Pac-Man**, el cual he construido utilizando el framework de interfaz gráfica Avalonia UI en C# y la poderosa plataforma .NET 9.0. Mi principal objetivo con este proyecto es alcanzar un estado de réplica "Arcade Perfect", recreando lo más fielmente posible todos los elementos de comportamiento e inteligencia artificial (IA) de los fantasmas originales usando técnicas modernas de Programación Orientada a Objetos (POO).

## Arquitectura y Módulos
He diseñado el proyecto utilizando una arquitectura separada para facilitar el mantenimiento y la escalabilidad del código:
- **Models (Dominio)**: Aquí escribí la lógica pura de las entidades (`Player`, `Ghost`, `GameMap`, `TileMap`). Es aquí donde codifiqué el motor matemático y la IA (Scatter, Chase, Frightened).
- **ViewModels (Aplicación)**: Este es el puente reactivo que engarza el estado de la partida y proporciona los comandos y los `ObservableProperty` necesarios a la vista.
- **Views (Presentación)**: Las he diseñado de forma declarativa utilizando archivos **AXAML** de Avalonia UI para manejar el dibujado fluido de celdas a píxeles.
- **Services (Infraestructura)**: Aquí abstraye los servicios pesados, como el sonido (con `LinuxSoundService`) para la reproducción directa de audios en mi entorno de desarrollo, y el guardado de puntajes.

---

## El Cerebro de los Fantasmas: Búsqueda del Camino Euclidiano

Para lograr que los fantasmas persigan a Pac-Man de forma autónoma e implacable, implementé un algoritmo basado en el **cálculo de posicionamiento de la Distancia Euclidiana**. Los fantasmas en mi juego no utilizan un algoritmo de búsqueda de nodos pesados (como A* o Dijkstra), ya que quería simular el comportamiento original y más ligero del hardware de 1980, pero con la precisión moderna. 

### ¿Cómo funciona el movimiento paso a paso?
El motor de IA de los fantasmas lo diseñé siguiendo una serie de pasos estrictos cada vez que llegan a una nueva baldosa o "Tile" del laberinto:

1. **Snap to Grid (Anclaje a la cuadrícula):**
Al usar velocidades de punto flotante (`double`), era común que un marco microscópico de distancia provocara que el fantasma rebotara contra la esquina de un pasillo al dar la vuelta. Para solucionarlo, aseguré de que cuando un fantasma está a punto de girar en una intersección plana, su posición física X e Y sea redondeada matemáticamente de manera exacta al centro de la nueva baldosa (`TileSize`). Esto les permite dar giros perfectos de 90 grados y calcular su próxima meta limpiamente desde el medio de la celda, previniendo que se atoren dando vueltas sobre sí mismos contra las paredes adyacentes.

2. **Detección de Salidas (Valid Directions):**
Una vez en el medio de la baldosa, el fantasma escanea cuáles son las direcciones permitidas para el próximo movimiento (arriba, abajo, izquierda, derecha). Por regla general que he programado (idéntica a la original), **los fantasmas no pueden dar media vuelta en 'U' espontáneamente**; esto fuerza a la IA a seguir adelante o doblar en esquinas, nunca retroceder de la nada salvo que ocurra un evento muy específico como comerse una Súper Píldora.

3. **Cálculo de la Distancia Mínima (Targeting Vector):**
Cada fantasma tiene su propio "Target" abstracto o cuadrícula meta temporal, que varía drásticamente dependiendo de su personalidad a través de un patrón *Strategy* (`IGhostAIStrategy`):
- **Blinky** (Rojo) pone el vector objetivo directamente en la coordenada actual de Pac-Man.
- **Pinky** (Rosa) dirige su vector a cuatro baldosas de distancia frente a la mirada y trayectoria de Pac-Man.
- **Inky** (Cyan) toma dos puntos focales usando a Blinky y a Pac-Man para intentar acorralar.
- **Clyde** (Naranja) hace persecución directa si está lejos, y si se acerca a menos de 8 tiles, se rinde y su vector objetivo se traslada de espaldas hacia la esquina inferior del mapa.

4. **El Teorema Euclidiano:**
Sabiendo su objetivo, el fantasma no calcula la ruta paso por paso dentro del laberinto para ver qué camino es el más corto. En cambio, simplemente revisa las intersecciones libres que tiene a su alrededor en ese preciso instante en la baldosa en la que está sentado, y para cada opción posible traza una línea diagonal directa imaginaria hasta su objetivo (usando la distancia euclidiana pitagórica: `Distancia² = (X2 - X1)² + (Y2 - Y1)²`). 

Finalmente, elije sin dudarlo la intersección que temporalmente ofrezca **la menor distancia en línea recta real a su foco objetivo**, sin importarle cuántos recovecos o paredes del laberinto haya en medio del pasillo que eligió. Elegí guardar la distancia únicamente al cuadrado porque al evitar calcular costosas raíces cuadradas a 60 cuadros por segundo logro optimizar drásticamente el rendimiento computacional de la partida.

---

## Bugs Clásicos del Arcade Resueltos por Modelado Físico

1. **El "Targeting Bug" de Pinky e Inky (Bug de suma vectorial en ensamblador)**:
   Pinky tiene un objetivo configurado apuntando a 4 casillas (tiles) frente a Pac-Man. Sin embargo, en la ROM original japonesa existía un bug estructural de registro asamblador cuando Pac-Man miraba hacia *Arriba*. No existía verificación perimetral: cuando querían calcular "4 casillas arriba", las celdas se desbordaban cruzadamente en código hacia el registro lateral, obteniendo el cálculo erróneo "4 casillas hacia arriba Y 4 casillas a la izquierda". Como verás en este código fuente, yo he modelado las direcciones en mi enumerador `MovementDirection.Up` apuntando estricta y matemáticamente como vector `(0, -1)`. La física e independencia que brindan mis tipos en C# prohíbe enteramente corromper un eje con el estado del otro, descartando algoritmos desfasados de forma definitiva.
