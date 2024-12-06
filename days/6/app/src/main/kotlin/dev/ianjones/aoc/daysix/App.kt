package dev.ianjones.aoc.daysix

import java.io.File
import java.util.Optional
import kotlin.system.exitProcess

fun main(args: Array<String>) {
    val inputLocation = if (args.isNotEmpty()) {
        args[0]
    } else {
        "./app/data/input.txt"
    }
    println(DaySix(InputLoader(inputLocation)))
}

class DaySix(loader: InputLoader) {
    private val data = loader.load()

    fun partOne(): Int {
        return PatrolMap(data).completePatrol().visitedCount
    }

    fun partTwo(): Int {
        return PatrolMap(data).computeAddedCycles()
    }

    override fun toString(): String {
        return "Part One: ${partOne()}\nPart Two: ${partTwo()}"
    }
}

class InputLoader(private val location: String) {

    fun load(): List<CharArray> {
        return File(location).readText().split('\n').filter(String::isNotEmpty).map { line -> line.toCharArray() }
    }
}

class PatrolMap(private val spec: List<CharArray>) {
    private val endSquare = MapSquare(
        isObstacle = false,
        coordinate = Coordinate(-1, -1),
        north = Optional.empty(),
        south = Optional.empty(),
        east = Optional.empty(),
        west = Optional.empty(),
        isVisited = false,
    )

    data class Coordinate(val x: Int, val y: Int)
    data class MappedSpec(val squares: List<List<MapSquare>>, val startSquare: MapSquare)
    private fun specToSquares(spec: List<CharArray>): MappedSpec {
        var startSquare: MapSquare = endSquare
        val squarifiedSpec = spec.mapIndexed { y, row ->
            row.mapIndexed { x, square ->
                val sq = MapSquare(
                    isObstacle = square == '#',
                    coordinate = Coordinate(x, y),
                    north = Optional.of(endSquare),
                    south = Optional.of(endSquare),
                    east = Optional.of(endSquare),
                    west = Optional.of(endSquare)
                )
                // this turns the function impure but it also saves another round of iteration
                if (square == '^') startSquare = sq
                sq
            }
        }

        assert(startSquare.coordinate != endSquare.coordinate) { "Never found a starting square, which is wild." }

        for ((y, row) in squarifiedSpec.withIndex()) {
            for ((x, square) in row.withIndex()) {
                val isNorthEdge = y == 0
                val isEastEdge = x == row.size - 1
                val isSouthEdge = y == squarifiedSpec.size - 1
                val isWestEdge = x == 0

                if (!isNorthEdge) square.north = Optional.of(squarifiedSpec[y - 1][x])
                if (!isEastEdge) square.east = Optional.of(squarifiedSpec[y][x + 1])
                if (!isSouthEdge) square.south = Optional.of(squarifiedSpec[y + 1][x])
                if (!isWestEdge) square.west = Optional.of(squarifiedSpec[y][x - 1])
            }
        }

        return MappedSpec(squarifiedSpec, startSquare)
    }

    init {

    }

    data class Movement(val square: MapSquare, val initialHeading: Cardinal, val finalHeading: Cardinal, val turnsCount: Int)
    data class TravelLog(val path: List<Movement>, val visitedCount: Int)

    fun completePatrol(): TravelLog {
        val (_, startSquare) = specToSquares(spec)
        val pathTaken = mutableListOf<Movement>()
        var visitedCount = 0
        var currentHeading = Cardinal.North
        var currentSquare = startSquare

        while (currentSquare != endSquare) {
            val initialHeading = currentHeading
            var turnsCount = 0
            if (!currentSquare.isVisited) {
                visitedCount += 1
                currentSquare.isVisited = true
            }

            var nextSquare = currentSquare.get(currentHeading)
            // obstacle case
            while (nextSquare.isObstacle) {
                currentHeading = turnRight(currentHeading)
                turnsCount += 1
                nextSquare = currentSquare.get(currentHeading)
            }
            pathTaken.add(Movement(square = currentSquare, initialHeading, finalHeading = currentHeading, turnsCount))
            // traversal case
            currentSquare = nextSquare

        }
        return TravelLog(pathTaken, visitedCount)
    }

    fun computeAddedCycles(): Int {
        var cycles = 0;
        for (y in spec.indices)
            for (x in spec[y].indices) {
                // make a new one, try to traverse it, at 10_000 steps it's a loop, ok?
                val specCopy = spec.map { it.copyOf() }
                if(specCopy[y][x] != '^' && specCopy[y][x] != '#') specCopy[y][x] = '#'
                val (_, startSquare) = specToSquares(specCopy)
                var stepsTaken = 0
                var currentSquare = startSquare
                var currentHeading = Cardinal.North
                while (currentSquare.coordinate != endSquare.coordinate) {
                    val (updatedHeading, updatedSquare) = currentSquare.step(currentHeading)
                    currentHeading = updatedHeading
                    currentSquare = updatedSquare
                    stepsTaken += 1
                    // assumption: taking over 10,000 steps is a loop
                    if (stepsTaken > 100_000) {
                        cycles += 1
                        break;
                    }
                }
            }
        return cycles
    }

    data class MapSquare(
        val isObstacle: Boolean,
        val coordinate: Coordinate,
        var north: Optional<MapSquare>,
        var south: Optional<MapSquare>,
        var east: Optional<MapSquare>,
        var west: Optional<MapSquare>,
        var isVisited: Boolean = false,
    ) {
        fun get(heading: Cardinal): MapSquare {
            return when (heading) {
                Cardinal.North -> north.orElseThrow()
                Cardinal.East -> east.orElseThrow()
                Cardinal.South -> south.orElseThrow()
                Cardinal.West -> west.orElseThrow()
            }
        }

        fun step(heading: Cardinal): Pair<Cardinal, MapSquare> {
            var next = get(heading)
            var currentHeading = heading
            while (next.isObstacle) {
                currentHeading = turnRight(currentHeading)
                next = get(currentHeading)
            }
            return Pair(currentHeading, next)
        }
    }


}

enum class Cardinal {
    North,
    South,
    East,
    West
}

fun turnRight(heading: Cardinal): Cardinal {
    return when (heading) {
        Cardinal.North -> Cardinal.East
        Cardinal.East -> Cardinal.South
        Cardinal.South -> Cardinal.West
        Cardinal.West -> Cardinal.North
    }
}
