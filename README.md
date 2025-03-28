# README doscuatro

1.- Instalar plugin TextMeshPro
2.- Instalar Plugin GUI Pro - Smple Casual

- AVANCE -

**MONOLITO**
  - Shoot pulsar to connected structures
  - Shoot every N seconds
  - shoot amount can be increased
  - the shoot time can be reduced (ticker limit)

  - can generate more types of pulsars that affect the structures hitted
  - it changed its form based on updates
  - Can shoot a wave that affects every sctucture ($$$)
      - the wave has a range that can be increased
      - special ability (?)  

**STRUCTURES**
  - they works as a dice with one face in the beginning
  - the random points are based on the amount of faces the structure has
  - the structure point start at 1
  - they receive the pulsar and user get a point
  - the amount of points that gives can only be "changed" when its clicked
  - When hit by a pulsar they shoot another pulsar to the connected structures
    - It need to remove the selected dice from the List of the other dice

  - diferent effects when hit by the pulsar based on the previous structure
      - need and upgrade
  - the sctucture can be upgraded to give more points
  - Dices throws stuff/parts when hit (?) 
      - based on percentage?
      - amount of hits?

**PLATFORM**
  - Structures can be installed on them
  - Only one structure per platform (?)
  - Structures moves between platforms instantly 

  - structure moves between platforms slowly with "animation"

**GUI**
  - show amount of currency
  - buy button
  - map creation button
  - delete map button

  - panel that control structure
  - 



### **El Culto del Monolito: Plan de Desarrollo**

---

### 🕰️ **Desarrollo por Semanas:**

**Semana 1-2:**  
- Prototipo básico del monolito que genera pulsos a intervalos definidos.  
- Creación de una escena simple en Unity con coordenadas nxn para colocar estructuras.  
- Implementación de la mecánica de disparo de pulsos.  

**Semana 3-4:**  
- Implementación de estructuras que reciben pulsos y envían pulsos secundarios a otras estructuras.  
- Diseño básico de interfaz para visualización de energía y progreso.  
- Creación de 2 estructuras iniciales (generadoras de recursos y convertidoras de pulsos).  

**Semana 5-6:**  
- Sistema de construcción y expansión del área alrededor del monolito.  
- Agregado de primeros upgrades para mejorar la distancia y velocidad de los pulsos.  

**Semana 7-8:**  
- Sistema de fe inicial (recolección de puntos de fe para mejoras).  
- Implementación de seguidores básicos con atributos aleatorios.  

**Semana 9-10:**  
- Implementación del sistema de prestigio.  
- Ascensión de seguidores a dioses con características únicas.  

---

### 🟦 **Monolito, Estructuras, Pulsos, Construcción y Expansiones:**

- **Monolito:**  
  - Centro del mapa en [0,0], lanza pulsos en intervalos.  
  - Pulsos pueden ser mejorados para aumentar alcance, velocidad y potencia.  

- **Estructuras:**  
  - Se colocan en coordenadas fijas en un sistema de cuadrícula nxn.  
  - Tipos iniciales:
    - Convertidor de pulsos → transforma pulsos en fe.  
    - Almacén → guarda energía espiritual para mejorar estructuras.  

- **Construcción y Expansiones:**  
  - Expansión del área controlada por el monolito para colocar nuevas estructuras.  
  - Actualización de estructuras para manejar más pulsos y mejorar producción.  

---

### 🎨 **Interfaz:**

- **Pantalla Principal:**  
  - Vista isométrica del monolito y estructuras.  
  - Indicadores de fe, energía y progreso del culto.  

- **Menú de Construcción:**  
  - Opciones para colocar estructuras y expandir el culto.  
  - Panel para ver y mejorar seguidores.  

- **Panel de Prestigio:**  
  - Lista de seguidores listos para ser ascendidos a dioses.  
  - Historial de dioses creados y sus efectos en el culto.  

---

### 🕊️ **Sistema de Fe:**

- **Generación de Fe:**  
  - Pulsos que llegan a estructuras convierten energía en fe.  
  - Almacenes de fe acumulan el recurso para gastar en mejoras.  

- **Uso de Fe:**  
  - Mejoras del monolito y estructuras.  
  - Ascensión de seguidores y desbloqueo de milagros.  

---

### ⚡ **Modo de Prestigio - Ascensión de Seguidores:**

- **Ascensión de Seguidores:**  
  - El jugador escoge un seguidor para convertirlo en dios al final del ciclo.  
  - Los dioses tienen características buenas y malas que afectan el culto.  

- **Nombres Importantes:**  
  - Algunos seguidores nacen con nombres importantes (basados en dioses antiguos).  
  - Estos dioses tienen habilidades poderosas pero pueden entrar en conflicto con otros.  

- **Relaciones entre Dioses:**  
  - Algunos dioses no funcionan bien juntos, causando malus si están activos al mismo tiempo.  

---

### ✨ **Sistema de Milagros y Poderes Místicos:**

- **Milagros:**  
  - Acciones puntuales que el jugador puede realizar para alterar el entorno.  
  - Ejemplo: aumentar la producción de fe, acelerar pulsos, o restaurar estructuras.  

- **Poderes Místicos:**  
  - Cada dios tiene poderes únicos que afectan el mundo de formas distintas.  
  - Los poderes pueden ser pasivos o activados por el jugador.  

---

### 📈 **Ideas Adicionales:**

- **Sistema de Eventos Aleatorios:**  
  - Eventos que afectan el culto positivamente o negativamente.  
  - Aparición de seguidores especiales o amenazas sobrenaturales.  

- **Relaciones entre Seguidores:**  
  - Seguidores pueden formar lazos, afectando la efectividad del culto.  
  - Relación positiva → bonificación de producción.  
  - Relación negativa → reduce la eficiencia.  

- **Generación Procedural de Mapas:**  
  - Cada partida tiene un mapa diferente, generando nuevos desafíos estratégicos.  

- **Sistema de Antorchas Sagradas:**  
  - Permite extender la influencia del monolito a zonas más lejanas.  

Estas ideas proporcionan una base sólida para un desarrollo progresivo y expandible del juego. 🚀