(do (assert (= 1 1) t "1 should equal 1")
    (assert (= 1 1 (- 2 1)) t "1 should equal 1 should equal 1")
    (assert (not (= 2 1)) t "2 should not equal 1"))