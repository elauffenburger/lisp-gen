# lisp-gen

lisp-gen is a library for implementing your own LISPs! It comes (...eventually!) with batteries included
(aka the necessary host bindings) so you can add your DSLs on top.

## Getting Started

Use `LispGen.Lib`, create a `LispGen.Lib.Scope` with `Scope.Root()` and start adding your own expression
bindings.

## Why Would I Use This?

You ~probably~ definitely shouldn't.

## Implemented

### Mathematics, Arithmetics, Logic and Comparisons
  - [x] `*`
  - [x] `+`
  - [x] `-`
  - [x] `/`
  - [ ] `/=`
  - [x] `1+`
  - [x] `1-`
  - [x] `<`
  - [x] `<=`
  - [x] `=`
  - [x] `>`
  - [x] `>=`
  - [x] `and`
  - [ ] `ceiling`
  - [ ] `cos`
  - [ ] `decf`
  - [ ] `eq`
  - [ ] `eql`
  - [ ] `equal`
  - [ ] `equalp`
  - [ ] `exp`
  - [ ] `expt`
  - [ ] `floor`
  - [ ] `incf`
  - [ ] `isqrt`
  - [ ] `logand`
  - [ ] `logior`
  - [ ] `max`
  - [ ] `min`
  - [ ] `mod`
  - [ ] `nil`
  - [x] `not`
  - [x] `or`
  - [ ] `random`
  - [ ] `round`
  - [ ] `sin`
  - [ ] `sqrt`
  - [x] `t`
  - [ ] `zerop`

### Conses, Lists and related functions
  - [ ] `append`
  - [ ] `assoc`
  - [ ] `butlast`
  - [ ] `car`
  - [ ] `cddr`
  - [ ] `cdr`
  - [ ] `cons`
  - [ ] `consp`
  - [ ] `first`
  - [ ] `getf`
  - [ ] `interserction`
  - [ ] `last`
  - [ ] `list`
  - [ ] `list-length`
  - [ ] `listp`
  - [ ] `mapc`
  - [ ] `mapcan`
  - [ ] `mapcar`
  - [ ] `mapcon`
  - [ ] `maplist`
  - [ ] `member`
  - [ ] `null`
  - [ ] `pop`
  - [ ] `push`
  - [ ] `pushnew`
  - [ ] `rest`
  - [ ] `rplaca`
  - [ ] `rplacd`
  - [ ] `second`
  - [ ] `set-difference`
  - [ ] `union`

### Sequences (Lists, Strings) and Arrays
  - [ ] `aref`
  - [ ] `concatenate`
  - [ ] `copy-seq`
  - [ ] `count`
  - [ ] `elt`
  - [ ] `find`
  - [ ] `length`
  - [ ] `make-array`
  - [ ] `make-sequence`
  - [ ] `map`
  - [ ] `map-into`
  - [ ] `position`
  - [ ] `reduce`
  - [ ] `remove`
  - [ ] `reverse`
  - [ ] `search`
  - [ ] `some`
  - [ ] `string`
  - [ ] `string-downcase`
  - [ ] `string-upcase`
  - [ ] `subseq`
  - [ ] `vector`
  - [ ] `vector-pop`
  - [ ] `vector-push`
  - [ ] `vector-push-extend`

### Symbol, Characters, Hash, Structure, Objects and Conversions
  - [ ] `atom`
  - [ ] `char-code`
  - [ ] `char-name`
  - [ ] `coerce`
  - [ ] `defstruct`
  - [ ] `digit-char-p`
  - [ ] `gensym`
  - [ ] `gethash`
  - [ ] `intern`
  - [ ] `make-hash-table`
  - [ ] `symbolp`

### Input and output
  - [ ] `format`
  - [ ] `get-output-stream-string`
  - [ ] `make-string-output-stream`
  - [ ] `read`
  - [ ] `read-char`
  - [ ] `read-line`
  - [ ] `write-string`

### Functions, Evaluation, Flow Control, Definitions and Syntax
  - [ ] `apply`
  - [ ] `block`
  - [ ] `case`
  - [ ] `cond`
  - [ ] `declare`
  - [ ] `defmacro`
  - [ ] `defparameter`
  - [~] `defun`
    - *Sorta; need support for doc comments and interactive arg-passing info*
  - [ ] `defvar`
  - [x] `do`
  - [ ] `dolist`
  - [ ] `error`
  - [ ] `eval`
  - [ ] `flet`
  - [ ] `funcall`
  - [ ] `function`
  - [ ] `if`
  - [ ] `labels`
  - [ ] `lambda`
  - [x] `let`
  - [ ] `let*`
  - [ ] `loop`
  - [ ] `progn`
  - [ ] `quote`
  - [ ] `return-from`
  - [ ] `setf`
  - [ ] `setq`
  - [ ] `unless`
  - [ ] `when`

### Non-Standard
  - [x] `assert`