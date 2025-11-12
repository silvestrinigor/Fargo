# Area

## Rules

1. A area can contain multiple entities, including other areas.
1. Entities can be in many areas.
1. Areas can be nested to create a hierarchy of items.
1. When a area is deleted, the entities in the area is passed to the parent area?
1. A 

# Article

## Rules

1. Will have informations about the product
1. Information about dimentions
1. Information about weight
1. Information about manufacturer
1. Information about autor
1. The article product can have `kg` information and `kg=12,123` information. If the `kg` value is not informed it means the article product dont have the concept of units.
1. The article product can have `dimension` information and `dimension=(x,y,z)`.If the `dimension` value is not informed it means the article product dont have the concept of units.


# ArticleBunch

## Rules

1. Every bunch of itens is related with one article.
1. The bunch of itens article can't change.
1. Inform quantity of itens in the bunch
1. Quantity depends of the bunch of itens article definition.
    - Example: if the user define the article as a product unit with 12kg, then the quantity should be per units or kg multiple of 12. 
    - If user defines the article as a product with dimentions x, y, z without informing the weight, then to define the bunch of itens quantity should be a cube information((x * y * z)^3)
1. Is possible to create a empty item bunch

# ArticleItem

## Rules

1. Every item is related with one article.
1. The item article can't change.
1. Item can't be created if the article don't have unit concept.

# Container

## Rules

1. A container can hold multiple entities, including other containers.
1. A entity can only belong to one container at a time.
1. Containers can be nested to create a hierarchy of items.
1. When a container is deleted, the entities in the container is passed to the parent container?
1. When a container `x` is inside a container `y`, a entity `z` is in `x`, container `y` contains `x` and `y`.
1. If a entity is added in a area that is inside a container, the entity should be inserted in the new container, and be removed from the previous container.
1. When a container is inside a area, if a entity is inserted in the container, should not be added in the area.
1. When a container `x` is inside a partition `y`, if a entity `z` is inserted in the container, should not necessary be added in the partition by default. If user is in partition view, then should be added in the partition.

# Partition

## Rules

1. A partition only be inserted inside other partitions?

# UserGroup

## Rules

# User

## Rules