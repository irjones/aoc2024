package dev.ianjones.aoc.daysix

import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertNotNull

class DaySixTest {
    @Test fun `it runs part one correctly with the test data`() {
        val daySix = DaySix(InputLoader("./data/testdata.txt"));
        assertEquals(41, daySix.partOne())
    }
    @Test fun `it runs part two correctly with the test data`() {
        val daySix = DaySix(InputLoader("./data/testdata.txt"));
        assertEquals(6, daySix.partTwo())
    }
}
