(do (let ((x 4)))
    (/= x 2)
    (assert (= 2 x) t "x should be 2 after dividing by 2"))