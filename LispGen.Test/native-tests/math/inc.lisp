(do (let ((x 1)))
    (let ((res (1+ x))))
    (assert (= 2 res) t "1+1 should be 2"))