# Container

An entity that can be used to hold or transport entities.

## Rules

1. A container can hold multiple entities, including other containers.
1. A entity can only belong to one container at a time.
1. Containers can be nested to create a hierarchy of items.
1. When a container is deleted, the entities in the container is passed to the parent container?
1. When a container `x` is inside a container `y`, a entity `z` is in `x`, container `y` contains `x` and `y`.
1. If a entity is added in a area that is inside a container, the entity should be inserted in the new container, and be removed from the previous container.
1. When a container is inside a area, if a entity is inserted in the container, should not be added in the area.
1. When a container `x` is inside a partition `y`, if a entity `z` is inserted in the container, should not necessary be added in the partition by default. If user is in partition view, then should be added in the partition.
