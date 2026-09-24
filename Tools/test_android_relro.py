"""Synthetic counterexamples for the RELRO diagnostic, not device tests."""
import struct
import unittest

from inspect_android_relro import LOAD, RELRO, analyze, outside_ranges


def elf(*segments):
    data = bytearray(0x10000)
    data[:7] = b"\x7fELF\x02\x01\x01"
    struct.pack_into("<HHI", data, 16, 3, 183, 1)
    struct.pack_into("<Q", data, 32, 64)
    struct.pack_into("<HHH", data, 52, 64, 56, len(segments))
    for index, (kind, addr, size, align) in enumerate(segments):
        offset = addr % 0x4000
        struct.pack_into("<IIQQQQQQ", data, 64 + index * 56,
                         kind, 6, offset, addr, addr, size, size, align)
    return data


class RelroTests(unittest.TestCase):
    def test_separate_rw_load_with_padding_does_not_report_writable_overlap(self):
        r = analyze(elf((LOAD, 0x8F20, 0x10E0, 0x4000),
                        (LOAD, 0xD150, 0x40, 0x4000),
                        (RELRO, 0x8F20, 0x10E0, 1)))
        self.assertFalse(r["rawRelroEnds16KbAligned"])
        self.assertFalse(r["declaredGeometryIssuesFound"])
        self.assertEqual(r["relro"][0]["roundedEnd"], "0xc000")

    def test_writable_tail_in_same_load_is_reported(self):
        r = analyze(elf((LOAD, 0x8F20, 0x2000, 0x4000),
                        (RELRO, 0x8F20, 0x10E0, 1)))
        self.assertEqual(r["relro"][0]["writableBytesOutsideRelro"],
                         [dict(loadIndex=0, start="0xa000", end="0xaf20")])

    def test_start_rounding_can_cover_writable_bytes_too(self):
        r = analyze(elf((LOAD, 0x8000, 0x4000, 0x4000),
                        (RELRO, 0x9000, 0x3000, 1)))
        self.assertTrue(r["rawRelroEnds16KbAligned"])
        self.assertTrue(r["declaredGeometryIssuesFound"])
        self.assertEqual(r["relro"][0]["writableBytesOutsideRelro"][0]["start"], "0x8000")

    def test_bss_counts_as_writable_memory(self):
        data = elf((LOAD, 0x8F20, 0x10E0, 0x4000), (RELRO, 0x8F20, 0x10E0, 1))
        struct.pack_into("<Q", data, 64 + 40, 0x2000)  # p_memsz > p_filesz
        self.assertTrue(analyze(data)["declaredGeometryIssuesFound"])

    def test_low_alignment_and_incongruence_are_reported(self):
        self.assertTrue(analyze(elf((LOAD, 0x8000, 0x4000, 0x1000)))["loadIssues"])
        data = elf((LOAD, 0x8000, 0x4000, 0x4000))
        struct.pack_into("<Q", data, 64 + 8, 1)  # p_offset
        self.assertTrue(analyze(data)["loadIssues"])

    def test_union_handles_nested_and_adjacent_ranges(self):
        self.assertEqual(outside_ranges(1, 12, [(3, 5), (5, 10), (4, 6)]), [(1, 3), (10, 12)])

    def test_missing_load_or_truncated_elf_fails_closed(self):
        for data in (b"ELF", elf((LOAD, 0x8000, 0x4000, 0x4000))[:80],
                     elf((RELRO, 0x8000, 0x4000, 1))):
            with self.assertRaises(ValueError):
                analyze(data)

    def test_invalid_segment_bounds_fail_closed(self):
        data = elf((LOAD, 0x8000, 0x4000, 0x4000))
        struct.pack_into("<Q", data, 64 + 32, 0x20000)  # p_filesz
        with self.assertRaises(ValueError):
            analyze(data)


if __name__ == "__main__":
    unittest.main()
