# IAV-PFinal

# IAV - Documento de Producción de la Práctica FInal

<br>

## Autor
- Pedro Sánchez Vela ([PedroSanchezPresidente](https://github.com/PedroSanchezPresidente))

## Propuesta
> [!NOTE]
> He cambiado la parte de los objetivos moviles de esta sección debido a que malinterpreté el código del libro y solo sirve para objetivos estáticos,
> ya que se consigue el ángulo de tiro dependiendo unicamente de todos los factores que afentan a la bala.

Esta práctica será sobre el capítulo 3.5 "Predicting Physics" de la tercera edicion del libro "AI for Games" de IAN Millintog.

La práctica consistirá en desarrollar un prototipo de IA para Videojuegos, sera una simulación de tiro al blanco con objetivos státicos, caida de bala y el cañon que la dispara se podrá mover en movimiento.
La IA a implementar tendrá que ajustar el ángulo de disparo, prediciendo la caida de bala y la resistencia del viento.

Los apartados a tratar son:
<table>
<tr><th>A</th><th>Crear un escenario con dianas en distintas alturas y con almenos una detrás de un muro. Tambíen una plataforma por donde se moverá el cañón y el propio cañon. El caños se moverá con las teclas A(izquierda) y D(derecha).</th></tr>
<tr><th>B</th><th>Crear una interfaz con la que puedas cambiar la diana a la que disparas, la velocidad de salida de la bala, la resistencia del aire, la gravedad y si se quiere que se lance el proyectil con la trayectoria mas alta posible.</th></tr>
<tr><th>C</th><th>Hacer que el cañon dispare un proyectil prediciendo el ángulo de salida para que alcanze el objetivo teniendo en cuenta la gravedad.</th></tr>
<tr><th>D</th><th>Añadir el drag que se desea a la bala y corregir el ángulo de tiro para que la bala alcanze el objetivo teniendo en cuenta la gravedad y la resistencia del aire.</th></tr>
<tr><th>E</th><th>Hacer un contador que aumente al impactar a un objetivo y scripts necesarios para eliminar las balas que ya han impactado.</th></tr>
</table>

<br>

## Punto de partida
Se parte de un proyecto vacío de **Unity 2022.3.5f1**. 

Los modelos de las dianas y los cañones, además de los script de movimiento son originales del autor.

El diagrama de como debería ser es el siguiente:

![diagrama_IAV](https://github.com/PedroSanchezPresidente/IAV-PFinal/assets/60969767/167bee76-867d-4526-a064-75469ff0de46)

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

<br>

![UML_PFINAL_IAV](https://github.com/PedroSanchezPresidente/IAV-PFinal/assets/60969767/98f15bd8-e0a3-40e7-8a3c-1bc3004f629b)

<br>
En la clase CanonComponent estará la mayoria del codigo con la IA de disparo.
Las demas son simplemente para limpiar las balas.
<br><br>

#### Calcular disparo
<br>
Diagrama con la trayectoria de disparo solo con la gravedad, como se puede observar hay 2 posibles trayectorias:
<br><br>

![driagrama_Gravity_IAV](https://github.com/PedroSanchezPresidente/IAV-PFinal/assets/60969767/f87aa751-a065-468d-8866-9485d05a99e0)

<br>
Para calcular la trayectoria del disparo, teniendo en cuenta la parabola
producida por la fuerza de la gravedad, se usara el siguiente pseudo codigo:
<br>

```
function calculateFiringSolution(start: Vector, end: Vector, muzzleV: float, gravity: Vector) -> Vector:
  # Calculate the vector from the target back to the start.
  delta: Vector = start - end

  # Calculate the real-valued a,b,c coefficients of a
  # conventional quadratic equation.
  a = gravity.squareMagnitude()
  b = -4 * (dotProduct(gravity, delta) + muzzleV * muzzleV)
  c = 4 * delta.squareMagnitude()

  # Check for no real solutions.
  b2minus4ac = b * b - 4 * a * c
  if b2minus4ac < 0:
    return null

  # Find the candidate times.
  time0 = sqrt((-b + sqrt(b2minus4ac)) / (2 * a))
  time1 = sqrt((-b - sqrt(b2minus4ac)) / (2 * a))

  # Find the time to target.
  if time0 < 0:
    if time1 < 0:
      # We have no valid times.
      return null
    else:
      ttt = time1
  else:
    if time1 < 0:
      ttt = time0
    else:
      ttt = min(time0, time1)

  # Return the firing vector.
  return (delta * 2 - gravity * (ttt * ttt)) / (2 * muzzleV * ttt)
```
<br><br>
#### Refinar ángulo
<br>
Diagrama con la trayectoria de disparo con gravedad y resistencia del aire:
<br><br>

![diagrama_Drag_IAV](https://github.com/PedroSanchezPresidente/IAV-PFinal/assets/60969767/2e7f7b6d-d74c-4f6b-b35e-8879efeb4413)

<br>
El siguiente pseudocodigo sirve para reajustar el ángulo de disparo en caso de haber una fuerza externa ejerciendo sobre el proyectil, por ejemplo, resistencia del aire:
<br>

```
function refineTargeting(start: Vector, end: Vector, muzzleV: float, gravity: Vector, margin: float) -> Vector:

  # Calculate a firing solution based on a firing angle.
  function checkAngle(angle):
    deltaPosition: Vector = target - source
    direction = convertToDirection(deltaPosition, angle)
    distance = distanceToTarget(direction, source, target, muzzleV)
    return direction, distance

  # Take an initial guess using the dragless firing solution.
  direction: Vector = calculateFiringSolution( source, target, muzzleVelocity, gravity)

  # Check if this is good enough.
  distance = distanceToTarget(direction, source, target, muzzleV)
  if -margin < distance < margin:
    return direction
  
  # Otherwise we will binary search, but we must ensure our minBound
  # undersoots and our maxBound overshoots.
  angle: float = asin(direction.y / direction.length())
  if distance > 0:
    # We’ve found a maximum bound. Use the shortest possible shot
    # (shooting straight down) as the minimum bound.
    maxBound = angle
    minBound = - pi / 2
    direction, distance = checkAngle(minBound)
    if -margin < distance < margin:
      return direction

  # Otherwise we need to check we can find a maximum bound: maximum
  # distance is achieved when we fire at 45 degrees = pi / 4.
  else:
    minBound = angle
    maxBound = pi / 4
    direction, distance = checkAngle(maxBound)
    if -margin < distance < margin:
      return direction

  # Check if our longest shot can’t make it.
  if distance < 0:
    return null

  # Now we have a minimum and maximum bound, so binary search.
  distance = infinity
  while abs(distance) >= margin:
    angle = (maxBound - minBound) / 2
    direction, distance = checkAngle(angle)
  
    # Change the appropriate bound.
    if distance < 0:
      minBound = angle
    else:
      maxBound = angle

  return direction
```
<br>
Aunque no lo parezca esta parte es muy compleja, ya que no hay una forma efectiva de predecir la trayectoria sin antes hacer una simulacion del recorrido.
Sirve para las IAs de enemigos en simuladores de guerra muy complejos y con nivel de detalle alto.
Esta predicción se hace en el metodo distanceToTarget y devuelve la distancia más cercana que pasará el proyectil al objetivo con signo.
Este signo será positivo si se ha pasado de largo y negativo si se ha quedado corto.
Millington no explica este método y es crucial para que funcione, solo dice lo anteriormente dicho por lo que he tenido que hacer investigación en internet.<br><br>
Además hay un pequeño fallo justo después de la linea "angle = (maxBound - minBound) / 2" y lo corregí poniendo justo después "angle = angle + minBound".
Porque sino hay posibilidad de que se quede dentro del while infinitamente, ya que calcula el ángulo intermedio de la diferencia pero no es el ángulo real y podría darse el caso que angle = minBound dando lugar a un while infinito.
<br><br>
El siguiente pseudocodigo sirve para convertir una posición y un ángulo a una dirección.
<br>

```
function convertToDirection(deltaPosition: Vector, angle: float):
  # Find the planar direction.
  direction = deltaPosition
  direction.y = 0
  direction.normalize()

  # Add in the vertical component.
  direction *= cos(angle)
  direction.y = sin(angle)
  
  return direction
```
<br><br><br>

## PRUEBAS Y METRICAS
> [!NOTE]
> Las nuevas pruebas harán énfasis en los cambios en el comportamiento de la bala y no de los objetivos.

Se harán las siguientes pruebas:

<table>
  <tr><th>Cañon fijo con objetivos a la misma altura</th></tr>
  <tr><th>Cañon fijo con objetivos a una altura superior</th></tr>
  <tr><th>Cañon fijo con objetivos escondidos detrás de un muro</th></tr>
  <tr><th>Cañon en movimiento con objetivos a la misma altura</th></tr>
  <tr><th>Cañon en movimiento con objetivos a una altura superior</th></tr>
  <tr><th>Cañon en movimiento con objetivos escondidos detrás de un muro</th></tr>
  <tr><th>Todas las pruebas anteriores pero la bala tiene resistencia al viento</th></tr>
  <tr><th>Todas las pruebas pero con distintas velocidades de bala</th></tr>
  <tr><th>Todas las pruebas pero con distintas gravedades</th></tr>
</table>

<br>
Hay una posibilidad muy pequeña de que falle el tiro cuando hay resistencia del viento 
porque el margen de error no está lo suficientemente ajustado.

<br><br>
[Video con la bateria de pruebas](https://www.youtube.com/watch?v=BgXPn1oNfJQ)
<br><br><br>

## Producción

Como esta practica es individual solo se usara la tabla de abajo para el seguimiento de las tareas pendientes.

| Estado  |  Tarea  |  Fecha  |  
|:-:|:--|:-:|
| ✔ | Diseño: Documentación inicial | 16-05-2024 |
| ✔ | Plantilla con la base del proyecto | 20-05-2024 |
| ✔ | Implementacion del codigo con la IA | 24-05-2024 |
| :x: | Pruebas | 27-05-2024 |
| :x: | Vídeo | 27-05-2024 |


<br><br>

## Referencias

Los recursos de terceros utilizados son de uso público.
>[!NOTE]
>ChatGPT no ha sido usado para crear código, sino para buscar enfoques y
>formulas relaccionadas con la funcionalidad de la funcion distanceToTarget.

- *AI for Games*, Ian Millington.
- UNITY ver 2022.3.5f1
- [Federico Peinado, Prueba Individual, Narratech](https://narratech.com/es/docencia/prueba/)
- [Foro de Unity donde hablan sobre fórmulas del drag](https://forum.unity.com/threads/drag-factor-what-is-it.85504/)
- [Documentacion Unity](https://docs.unity3d.com)
- [ChatGPT](https://chatgpt.com)
- [Arma 3](https://store.steampowered.com/app/107410/Arma_3/?l=spanish)
- [War Thunder](https://store.steampowered.com/app/236390/War_Thunder/?l=spanish)
- [From the Depths](https://store.steampowered.com/app/268650/From_the_Depths/?l=spanish)
