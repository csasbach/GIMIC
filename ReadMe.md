GIMIC
=====

GIMIC
Isn't
Markup
It's
Configuration

8 STEP SPEC:
------------

1.  A GIMIC file is a newline delimitted list of key-value pairs. 
2.  Everything before the first colon in a line is the key. 
3.  Everything after the first colon in a line is the value.
4.  Leading and trailing tabs and spaces of keys and values are always stripped.
6.  Starting your value with a newline and at least one more tab than the previous line started with
    creates a 'map' value which is itself a newline delimitted list of key-value pairs.
7.  A newline that starts with at least one less tab than the previous line started with closes the last map that was created that is still open.
5.  Putting commas in your value creates a comma-separated list of values. Note that since this is a list of values and not a list of key-value pairs, a list cannot contain a map.
8.  Putting two back-ticks (``) before and after anything means "Ignore all the other rules". Then the pairs of back-ticks are stripped.

EASY TO READ:
-------------

```
Name: Jane Smith
Dependants: Jim Smith, Jenny Smith, Jared Smith
Address:
    City: ``Omaha, NE``
    Street: 1234 N Wonder Rd.
    Apt: 87654
Phone Number: 555-1234
Date Joined: 2019/01/31T15:00:00-6
Days of vacation accrued: 14.5
Days of vacation used: 8
Administrative Metadata: ``{"username":"smijan01", "org":"Data Processing"}``
```

EASY TO PARSE:
--------------

1.  Parse the GIMIC document following the 8 step spec above.
3.  To GIMIC, the keys and values are just strings, but you can parse them further into whatever you like however you like.
2.  Store the parsed keys and values into your favorite dictionary, map, or object data structure.