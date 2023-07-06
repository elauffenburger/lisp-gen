(do (assert (>= 2 1) t "2 should be > 1")
    (assert (>= 2 2) t "2 should be = 2")
    (assert (not (>= 2 3)) t "2 should not be > 3"))