(do (assert (<= 1 2) t "1 should be less than 2")
    (assert (<= 2 2) t "2 should equal 2")
    (assert (not (<= 2 1)) t "2 should be less than 1"))