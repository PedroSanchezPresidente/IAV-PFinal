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

## Punto de partida
Se parte de un proyecto vacío de **Unity 2022.3.5f1**. 

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

<br>

![UML_PFINAL_IAV](https://github.com/PedroSanchezPresidente/IAV-PFinal/assets/60969767/9f54a841-fd77-42d5-a513-44ec308a881f)

<br>
En la clase ShootComponent estará la mayoria del codigo con la IA de disparo.
<br><br>

#### Calcular disparo

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
</table>

<br><br><br>

## Producción

Como esta practica es individual solo se usara la tabla de abajo para el seguimiento de las tareas pendientes.

| Estado  |  Tarea  |  Fecha  |  
|:-:|:--|:-:|
| ✔ | Diseño: Documentación inicial | 16-05-2024 |
| ✔ | Plantilla con la base del proyecto | 20-05-2024 |
| :x: | Implementacion del codigo con la IA | 24-05-2024 |
| :x: | Vídeo | 27-05-2024 |
| :x: | Pruebas | 27-05-2024 |


<br><br>

## Referencias

Los recursos de terceros utilizados son de uso público.

- *AI for Games*, Ian Millington.
- UNITY ver 2022.3.5f1
