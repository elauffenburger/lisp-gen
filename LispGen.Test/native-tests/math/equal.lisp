(do 
    ;; numbers
    (assert (equal 1 1))
    (assert (equal 2 (+ 1 1)))
    (assert (not (equal 2 1)))

    ;; lists
    (assert (equal '(1 2 3) '(1 2 3)))
    (assert (not (equal '(1 2 3) '(1 2))))
)