(do (assert (equal '(1 2 3) (list 1 2 3)))
    (assert (equal '(1 2 3) (list 1 (+ 1 1) 3))))