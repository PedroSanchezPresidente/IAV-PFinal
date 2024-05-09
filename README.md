# IAV-PFinal

# IAV - Documento de Producción de la Práctica FInal

<br>

## Autor
- Pedro Sánchez Vela ([PedroSanchezPresidente](https://github.com/PedroSanchezPresidente))

## Propuesta

Esta práctica será sobre el capítulo 3.5 "Predicting Physics" de la tercera edicion del libro "AI for Games" de IAN Millintog.

La práctica consistirá en desarrollar un prototipo de IA para Videojuegos, sera una simulación de tiro al blanco con objetivos moviles, caida de bala y el cañon que la dispara también estrara en movimiento.
La IA a implementar tendrá que ajustar el angulo de disparo, prediciendo la caida de bala y posicion del objetivo para cuando la bala llege.

## Punto de partida
Se parte de un proyecto vacio de **Unity 2022.3.5f1**. 

Los modelos de las dianas y los cañones, además de los script de movimiento son originales del autor.

<br><br>
## Guía de estilo del código
Para dar cohesión al trabajo, se ha acordado el uso de unas directrices que seguir a la hora de programar el código (que no el pseudocódigo). Estas son:
- El uso de Camel Case para las variables, ya sean privadas o públicas. Un ejemplo de esto sería: ```int varName```.
- El uso de Pascal Case para la declaración y uso de clases. Por ejemplo: ```class MyClass```. Además de para las funciones, ya que sigue el protocolo de C# para Unity. Por ejemplo: ```void MyFunction()```.
  
Siguiendo con la misma línea, se han seguido las directrices marcadas por el estilo del código base proporcionado para una mayor homogeneidad. El idioma del código es español mayoritariamente. En algunas excepciones se ha usado inglés, siguiendo con la estructura y forma del código base. Lo ideal es que todas las funciones vayan acompañadas de un comentario que describa brevemente su código a no ser que su nombre o brevedad sean autoexplicativos.

<br><br>
## Diseño de la solución
<br>
El diseño de la solución se puede visualizar en el siguiente UML, en el que se encuentran las principales funciones de cada clase. Se ha obviado añadir los atributos y variables para una visualización más clara y precisa. Se busca que el UML aporte claridad a cómo heredan las clases entre sí y cómo está distribuida la funcionalidad. <br><br>

> [!NOTE]
> En el editor no dejaba añadir textos para poner los metodos de las clases pero casi todas las clases tienen unicamente un metodo check que devuelve un booleano.
<br>

![Práctica 1 UML](https://github.com/IAV24-G10/IAV24-G10-P3/assets/60969767/1d85040c-a766-43ad-acdb-3ea3e9067bfa)<br>

Lo que vamos a realizar para resolver esta práctica son varios modelos de pseudocódigo de percepción por vista u oído, movimiento, dirección, llegada, desplazamiento, merodeo y navegación mediante A*. Todos ellos en movimiento dinámico descritos en la documentación de la [Práctica 1](https://github.com/IAV24-G10/IAV24-G10-P1) y [Práctica 3 de IAV](https://github.com/IAV24-G10/IAV24-G10-P3).
<br><br>
#### Maquina de estados de los guardas
<br>

                    +------------------+           +------------------+
                    |     Patrulla     | --------> |   Persecución    |
                    +------------------+   ve a    +------------------+
                           ^  |                  |   ^          |
                           |  |                  |   |          | cerca
         recarga completa  |  |  sin munición    |   |          v
                           |  |                  |   |    +----------------+
                           |  |                  +---+    |     Ataque     |
                           |  +-------------------------> +----------------+
                           |           pierde a Néstor           |  ^
                           |                                     |  | Néstor escapa
                           |                                     |  |
                           |                                     v  |
                     +-------------------+              sin munición |
                     |      Recarga      | <------------------------+
                     +-------------------+

<br>

#### Árbol de decisiones

<br>
Los nodos dentro del árbol de deciones pueden ser decisiones o acciones.
<br>

Para los nodos del árbol de decisiones usaremos como base la clase:
<br>

```
class DecisionTreeNode:
  function makeDecision() -> DecisionTreeNode # Recursively walks through the tree
```

<br><br>
Para las acciones usaremos como base la siguiente clase:
<br>
```
class Action extends DecisionTreeNode:
  function makeDecision() -> DecisionTreeNode:
    return this
```
<br><br>
Para las decisiones usaremos como base la siguiente clase:
<br>

```
class Decision extends DecisionTreeNode:
  trueNode: DecisionTreeNode
  falseNode: DecisionTreeNode
  function testValue() -> any
  function getBranch() -> DecisionTreeNode # carries out the test
  function makeDecision() ->DecisionTreeNode # Recursively walks through the tree
```
<br><br>
Especializamos la clase decision:
<br>
```
class FloatDecision extends Decision:
  minValue: float
  maxValue: float

  function testValue() -> float
  
  function getBranch() -> DecisionTreeNode:
    if maxValue >= testValue >= minValue:
      return trueNode
    else:
      return falseNode
```
<br><br>
Definimos el método makeDecision para que sea recorsiva y recorra el árbol de arriba a abajo:
<br>
```
class Decision extends DecisionTreeNode:
  function makeDecision() -> DecisionTreeNode:

    # Make the decision and recurse based on the result
    branch: DecisionTreeNode = getBranch()
    return branch.makeDecision()
```

<br><br>
La siguiente clase sirve para arboles con múltiples ramas:
<br>
```
class MultiDecision extends DecisionTreeNode:
  daughterNodes: DecisionTreeNode

  function testValue() -> int

  # Carries out the test and returns the node to follow
  function getBranch() -> DecisionTreeNode:
    return daughterNodes[testValue]

  # Recursively runs the algorithm, exactly as before
  function makeDecision() -> DecisionTreeNode:
    branch: DecisionTreeNode = getBranch()
    return branch.makeDecision()
```

<br><br>
#### Máquina de estados
<br>
El siguiente pseudocodigo representa una maquina de estados:
<br>

```
class StateMachine:

  # Holds the initial state
  initialState: State

  # Holds the current state
  currentState: State = initialState

  # Checks and applies transitions, returning a list of
  # actions.
  function  update() -> Action[]:

    # Assume no transition is triggered
    triggered: Transition = null
  
    # Check through each transition and store the first
    # one that triggers.
    for transition in currentState.getTransitions():
      if transition.isTriggered():
        triggered = transition
        break
  
    # Check if we have a transition to fire
    if triggered:
      # Find the target state
      targetState = triggered.getTargetState()
  
      # Add the exit action of the old state, the
      # transition action and the entry for the new state.
      actions = currentState.getExitAction()
      actions += triggered.getAction()
      actions += targetState.getEntryAction()
    
      # Complete the transition and return the action list
      currentState = targetState
      return actions
    
    # Otherwise just return the current state’s actions
    else:
      return currentState.getAction()
```
<br>
La maquina de estados esta formado por estados y transiciones. Se usará para definir los estados de los enemigos:
<br><br>

```
class State:
  function getAction() -> Action[]
  function getEntryAction() -> Action[]
  function getExitAction() -> Action[]
  function getTransitions() -> Transition[]
```
<br>

```
class Transition:
  actions: Action[]
  function getAction() -> Action[]:
    return actions

  targetState: State
  function getTargetState() -> State:
    return targetState

  condition: Condition
  function isTriggered() -> bool:
    return condition.test()
```
<br>
La siguiente clase es la base para la clase condicion
<br><br>

```
class Condition:
  function test() -> bool
```
<br>
Las siguientes clases representan las posible condiciones. (Not, And, Or y si un float esta en un rango):
<br><br>

```
class FloatCondition extends Condition:
  minValue: float
  maxValue: float

  function testValue() -> float # Pointer to the game data we’re interested in

  function test() -> bool:
    return minValue <= testValue <= maxValue

class AndCondition extends Condition:
  conditionA: Condition
  conditionB: Condition

  function test() -> bool:
    return conditionA.test() and conditionB.test()

class NotCondition extends Condition:
  condition: Condition
  
  function test() -> bool:
    return not condition.test()

class OrCondition extends Condition:
  conditionA: Condition
  conditionB: Condition

  function test() -> bool:
    return conditionA.test() or conditionB.test()
```
<br><br>

#### Árbol de comportamiento
<br>
La base para las tareas
<br>

```
class Task:
  function run -> bool
```
<br><br>
Si las tareas se van realizando de forma consecutiva  
<br>

```
class Selector extends Task:
  children: Task[]

  function run() -> bool:
    for c in children:
      if c.run()
        return true
      return false
```
<br><br>
Si las tareas se van realizando de forma secuencial
<br>

```
class Sequence extends Task:
  children: Task[]

  function run() -> bool:
    for c in children:
      if not c.run()
        return false
      return true
```
<br><br>
Esta seria la explicacion del esquema de la maquina de estados
<br>
```
Árbol de Comportamiento de Néstor:

Selector Principal

Asegura que se tomen decisiones a alto nivel para cambiar entre comportamientos críticos.Secuencia (Escapar)
  Prioriza el escape si es posible y seguro.
  - Selector (Camino Seguro a la Salida)
      - Tarea (Buscar Camino Libre de Guardias)
      - Tarea (Usar Escondites)
    - Tarea (Mover hacia la Salida)

  - Secuencia (Recargar Energía)
    Maneja la energía de Néstor, crucial para su movilidad y capacidad de escape.
      - Condición (Energía Baja)
      - Selector (Obtener Energía)
          - Tarea (Ir a Cápsula de Energía más Cercana)
          - Tarea (Esperar/Buscar Nueva Cápsula)

  - Secuencia (Evitar Guardias)
    Toma acciones cuando los guardias están cerca o han detectado a Néstor.
      - Condición (Guardia Cercano)
      - Selector (Evadir o Esconderse)
          - Tarea (Usar Escondites)
          - Tarea (Mover Hacia Zona Segura)

  - Secuencia (Interacción con Botones y Puertas)
    Maneja la interacción con elementos del entorno que son esenciales para abrir rutas o bloquear seguimientos.
      - Condición (Puerta Cerrada en la Ruta)
      - Selector (Operar Botones)
          - Tarea (Ir al Botón Correspondiente)
          - Tarea (Activar Botón)

Explicación del Árbol
     - Selector Principal: Es el nodo raíz que determina cuál secuencia de comportamientos ejecutar basada en la situación
                           actual. Este selector siempre evaluará en orden para priorizar el escape sobre otras acciones,
                           a menos que la salud de Néstor esté críticamente baja o haya una amenaza inminente de un guardia.

     - Secuencia (Escapar): Este comportamiento se activa si hay un camino claro y seguro hacia la salida. Utiliza un
                            sub-selector para determinar el mejor camino y procede a mover a Néstor hacia la salida.

     - Secuencia (Recargar Energía): Si la energía de Néstor es baja, este comportamiento toma prioridad (a menos que un
                                     escape seguro esté inmediatamente disponible). Busca la cápsula de energía más cercana
                                     o espera a que una esté accesible si todas están lejos o en áreas de alto riesgo.

     - Secuencia (Evitar Guardias): Activa cuando hay guardias cerca. Néstor intentará usar escondites o moverse a zonas fuera
                                    del alcance visual de los guardias para evitar la detección y ataque.

     - Secuencia (Interacción con Botones y Puertas): Cuando una ruta está bloqueada por una puerta, esta secuencia guía a
                                                      Néstor a operar botones para abrir la ruta o cerrarla detrás de él para
                                                      bloquear a los guardias.

```
<br>

Lo anterior explicado se puede visualizar en el siguiente grafo:

<br>

![Selector principal](https://github.com/IAV24-G10/IAV24-G10-P3/assets/95546683/34c9b9cc-bd11-41ee-b74d-9b51913b8694)

<br><br>

## Pruebas y métricas


Característica A: Mundo virtual
<table>
    <tr>
        <th><b>A.1</b></th>
        <th>Probar en el nivel 1 el movimiento de robot.</th>
    </tr>
     <tr>
        <th><b>A.2</b></th>
        <th>Probar que funciona el zoom en todos los niveles.</th>
    </tr>
    
</table>
<br>
Característica B: Puertas y escondites
<table>
    <tr>
        <th><b>B.1</b></th>
        <th>Las puertas, cuando están cerradas, bloquean tanto la visibilidad de los guardias como el movimiento de todos los robots. </th>
    </tr>
    <tr>
        <th><b>B.2</b></th>
        <th>Los escondites bloquean la visibilidad de los guardias.</th>
    </tr>
    <tr>
        <th><b>B.3</b></th>
        <th>Las paredes bloquean la visibilidad de los guardias.</th>
    </tr>
    
</table>
<br>
Característica C: Guardias
<table>
    <tr>
        <th><b>C.1</b></th>
        <th>Cuando están en idle, los guardias permanecen en un estado de patrulla empezando y acabando en su base. Comprobar en dos guardias distintos sus puntos de patrulla.</th>
    </tr>
    <tr>
        <th><b>C.2</b></th>
        <th>Si detectan al jugador con su campo de visión, los guardias empiezan a perseguirlo y dispararle.</th>
    </tr>
    <tr>
        <th><b>C.3</b></th>
        <th>Al matar al jugador o perderlo de vista, los guardias vuelven a patrullar.</th>
    </tr>
     <tr>
        <th><b>C.4</b></th>
        <th>Comprobar que al quedarse sin munición, los guardias vuelven a su base a recargar.</th>
    </tr>
</table>
<br>
Característica D: Árbol de comportamientos
<table>
    <tr>
        <th><b>D.1</b></th>
        <th>Probar si la IA es capaz de sacar a Néstor del escenario de ejemplo con vida.</th>
    </tr>
   <tr>
        <th><b>D.2</b></th>
        <th>Néstor cambia su rumbo y recarga vida cuando le queda menos de 1/3.</th>
    </tr>
</table>
<br>
Característica E: Memoria
<table>
    <tr>
        <th><b>E.1</b></th>
        <th>Teniendo memoria, la IA puede sacar a Néstor con vida del nivel 1.</th>
    </tr>
  <tr>
        <th><b>E.2</b></th>
        <th>Teniendo memoria, la IA puede sacar a Néstor con vida del nivel 2.</th>
    </tr>
  <tr>
        <th><b>E.3</b></th>
        <th>Teniendo memoria, la IA puede sacar a Néstor con vida del nivel 3 (nivel prueba).</th>
    </tr>
</table>
<br>

- [Vídeo con la batería de pruebas](https://youtu.be/Gc9cRzQK4xI)

Por si acaso no va el vídeo en youtube porque lleva una hora procesándose: 
[Enlace a drive](https://drive.google.com/file/d/1g9pz_VC4k-2DsYKUajGFlNIrj6LF4NeT/view?usp=sharing)

<br><br><br>



## Producción

Las tareas se han realizado y el esfuerzo ha sido repartido entre los autores. Esto queda documentado en la tabla siguiente de manera general, aunque se encuentra más profundamente documentado en la [pestaña de Proyectos actualizada](https://github.com/orgs/IAV24-G10/projects/3) de GitHub.

| Estado  |  Tarea  |  Fecha  |  
|:-:|:--|:-:|
| ✔ | Diseño: Documentación inicial | 18-04-2024 |
| ✔ | Característica A: Mundo virtual | 28-04-2024 |
| ✔ | Característica B: Puertas | 28-04-2024 |
| ✔ | Característica C: Guardias | 28-04-2024 |
| ✔ | Característica D: Protagonista | 28-04-2024 |
| ✔ | Característica E: Pizarra | 28-04-2024 |
| ✔ | Diseño: Documentación final | 28-04-2024 |
| :x: | Vídeo | 28-04-2024 |
|   |  | |
|  | OPCIONAL |  |
| :x: | Movimiento e interacción con ratón y WASD. Cambio con barra espaciadora | - |
| :x: | Máquina de estados jerárquica para los guardias | - |
| :x: | Escenario con geometría compleja 3D. Varios niveles. | - |
| :x: | Mecanismos más complejos de escenario (puertas giratorias, ascensores...) | - |
| :x: | Mejora de gestión sensorial del protagonista con retroalimentación visual | - |


<br><br>

## Referencias

Los recursos de terceros utilizados son de uso público.

- *AI for Games*, Ian Millington.
- [Behaviour Bricks Documentation](https://bb.padaonegames.com/)
- [Kaykit Medieval Builder Pack](https://kaylousberg.itch.io/kaykit-medieval-builder-pack)
- [Federico Peinado, Robot a la Fuga, Narratech](https://narratech.com/es/inteligencia-artificial-para-videojuegos/decision/robot-a-la-fuga/)
- [Federico Peinado, Representación del conocimiento, Narratech](https://narratech.com/es/inteligencia-artificial-para-videojuegos/decision/representacion-del-conocimiento/)
- [Fededrico Peinado, Máquina de estados, Narratech](https://narratech.com/es/inteligencia-artificial-para-videojuegos/decision/maquina-de-estados/)
- [Federico Peinado, Árbol de comportamiento, Narratech](https://narratech.com/es/inteligencia-artificial-para-videojuegos/decision/arbol-de-comportamiento/)
- [Federico Peinado, Reglas y planificación, Narratech](https://narratech.com/es/inteligencia-artificial-para-videojuegos/decision/reglas-y-planificacion/)
- [Federico Peinado, Probabilidad y utilidad, Narratech](https://narratech.com/es/inteligencia-artificial-para-videojuegos/decision/probabilidad-y-utilidad/)
