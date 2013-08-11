#include <cstdio>
#include <cstring>
#include <vector>
#include <set>
#include <string>
#include <unordered_set>
#include "rand.h"
#include <openssl/md5.h>
#define clr(a) memset(a, 0, sizeof(a))
typedef unsigned long long ll;

const int zero = 0;
const int one = 1;
const int var = 2;

const int shl1 = 3;
const int shr1 = 4;
const int shr4 = 5;
const int shr16 = 6;
const int not_op = 7;

const int and_op = 8;
const int or_op = 9;
const int xor_op = 10;
const int plus_op = 11;

const int if_op = 12;

const int var2 = 13;
const int var3 = 14;
const int fold = 14;

//const int max_size = 120000000;
const int max_size = 46000000;
//const int max_size = 200;
const int max_vals = 32;


typedef ll int_vec[max_vals];

std::vector<int> t[20];
struct expr
{
	int type;
	int lhs;
	int rhs;
	int if_cond;
} trees[max_size+1], trees2[max_size+1];

int_vec values;
int_vec values2;
int_vec values3;
int_vec *answers;
int_vec *answers2;

int_vec temp_answers;

const ll hash_mul = 10000000000037LL;

std::unordered_set<ll> hashes;
std::unordered_set<ll> hashes2;
bool has_hash(ll hash, int type1)
{
	if (type1)
	{
		if (hashes.find(hash) == hashes.end())
		{
			hashes.insert(hash);
			return false;
		}
		return true;
	}
	else
	{
		if (hashes2.find(hash) == hashes2.end())
		{
			hashes2.insert(hash);
			return false;
		}
		return true;
	}
}

bool last_eval;
bool eval(int x, bool type1)
{
	//return true;
	int left = type1 ? trees[x].lhs : trees2[x].lhs;
	int right = type1 ? trees[x].rhs : trees2[x].rhs;
	int if_c = type1 ? trees[x].if_cond : trees2[x].if_cond;
	switch(type1 ? trees[x].type : trees2[x].type)
	{
		case zero:
			for(int i = 0; i < max_vals; i++) temp_answers[i] = 0; 
			break;
		case one:
			for(int i = 0; i < max_vals; i++) temp_answers[i] = one; 
			break;
		case var:
			for(int i = 0; i < max_vals; i++) temp_answers[i] = values[i]; 
			break;
		case and_op:
			for(int i = 0; i < max_vals; i++) temp_answers[i] = answers[left][i] & answers[right][i];
			break;
		case or_op:
			for(int i = 0; i < max_vals; i++) temp_answers[i] = answers[left][i] | answers[right][i];
			break;
		case xor_op:
			for(int i = 0; i < max_vals; i++) temp_answers[i] = answers[left][i] ^ answers[right][i];
			break;
		case plus_op:
			for(int i = 0; i < max_vals; i++) temp_answers[i] = answers[left][i] + answers[right][i];
			break;
		case shl1:
			for(int i = 0; i < max_vals; i++) temp_answers[i] = answers[left][i] << 1;
			break;
		case shr1:
			for(int i = 0; i < max_vals; i++) temp_answers[i] = answers[left][i] >> 1;
			break;
		case shr4:
			for(int i = 0; i < max_vals; i++) temp_answers[i] = answers[left][i] >> 4;
			break;
		case shr16:
			for(int i = 0; i < max_vals; i++) temp_answers[i] = answers[left][i] >> 16;
			break;
		case not_op:
			for(int i = 0; i < max_vals; i++) temp_answers[i] = ~ answers[left][i];
			break;
		case if_op:
			for(int i = 0; i < max_vals; i++) temp_answers[i] = (answers[if_c][i] == 0) ? answers[left][i] : answers[right][i];
			break;
	}
	if (!type1)
		return true;
	memcpy(type1 ? answers[x] : answers2[x], temp_answers, sizeof(temp_answers));
	if (x >= max_size - 1)
		return last_eval = false;
	ll hash = 0;
	unsigned char charhash[16];
	MD5((const unsigned char*)(void*)temp_answers, sizeof(temp_answers), charhash);
	hash = * ((ll*)((void*)(charhash)));

	if (has_hash(hash, type1))
		return last_eval = false;

	return last_eval = true;
}

int_vec stack[50];
int ssize = 0;

void eval_lambda(int x)
{
	//return true;
	int left = trees2[x].lhs;
	int right = trees2[x].rhs;
	int if_c = trees2[x].if_cond;
	switch(trees2[x].type)
	{
		case zero:
			for(int i = 0; i < max_vals; i++) stack[ssize][i] = 0; 
			break;
		case one:
			for(int i = 0; i < max_vals; i++) stack[ssize][i] = one; 
			break;
		case and_op:
			ssize++; eval_lambda(left);
			ssize++; eval_lambda(right);
			ssize -= 2;
			for(int i = 0; i < max_vals; i++) stack[ssize][i] = stack[ssize+1][i] & stack[ssize+2][i];
			break;
		case or_op:
			ssize++; eval_lambda(left);
			ssize++; eval_lambda(right);
			ssize -= 2;
			for(int i = 0; i < max_vals; i++) stack[ssize][i] = stack[ssize+1][i] | stack[ssize+2][i];
			break;
		case xor_op:
			ssize++; eval_lambda(left);
			ssize++; eval_lambda(right);
			ssize -= 2;
			for(int i = 0; i < max_vals; i++) stack[ssize][i] = stack[ssize+1][i] ^ stack[ssize+2][i];
			break;
		case plus_op:
			ssize++; eval_lambda(left);
			ssize++; eval_lambda(right);
			ssize -= 2;
			for(int i = 0; i < max_vals; i++) stack[ssize][i] = stack[ssize+1][i] + stack[ssize+2][i];
			break;
		case shl1:
			ssize++; eval_lambda(left); ssize --;
			for(int i = 0; i < max_vals; i++) stack[ssize][i] = stack[ssize+1][i] << 1;
			break;
		case shr1:
			ssize++; eval_lambda(left); ssize --;
			for(int i = 0; i < max_vals; i++) stack[ssize][i] = stack[ssize+1][i] >> 1;
			break;
		case shr4:
			ssize++; eval_lambda(left); ssize --;
			for(int i = 0; i < max_vals; i++) stack[ssize][i] = stack[ssize+1][i] >> 4;
			break;
		case shr16:
			ssize++; eval_lambda(left); ssize --;
			for(int i = 0; i < max_vals; i++) stack[ssize][i] = stack[ssize+1][i] >> 16;
			break;
		case not_op:
			ssize++; eval_lambda(left); ssize --;
			for(int i = 0; i < max_vals; i++) stack[ssize][i] = ~ stack[ssize+1][i];
			break;
		case if_op:
			ssize++; eval_lambda(left);
			ssize++; eval_lambda(right);
			ssize++; eval_lambda(if_c);
			ssize -= 3;
			for(int i = 0; i < max_vals; i++) stack[ssize][i] = (stack[ssize+3][i] == 0) ? stack[ssize+1][i] : stack[ssize+2][i];
			break;
		case var2:
			for(int i = 0; i < max_vals; i++) stack[ssize][i] = values2[i] & 255; 
			break;
		case var3:
			for(int i = 0; i < max_vals; i++) stack[ssize][i] = values3[i]; 
			break;
		default:
			throw 42;
	}
}


int first[200];
int last[200];

bool good_op[20];

std::vector<std::string> operation_names = {"0", "1", "x", "shl1", "shr1", "shr4", "shr16", "not", "and", "or", "xor", "plus", "if0", "y", "z"};

void print_tree(int x, bool type1 = true)
{
	expr* trees = type1 ? ::trees : trees2;
	int type = trees[x].type;
	if (type >= 13)
	{
		printf("%s", operation_names[type].c_str());
		return;
	}
	if (type >= 3)
		printf("(");
	printf("%s", operation_names[type].c_str());
	if (type >= 12)
	{
		printf(" ");
		print_tree(trees[x].if_cond, type1);
	}
	if (type >= 3)
	{
		printf(" ");
		print_tree(trees[x].lhs, type1);
	}
	if (type >= 8)
	{
		printf(" ");
		print_tree(trees[x].rhs, type1);
	}
	if (type >= 3)
		printf(")");
}

void print_tree_root(int x)
{
	printf("(lambda (x) ");
	print_tree(x);
	printf(")\n");
}

char s[100000];

void read_operators()
{
	gets(s);
	int pos = 0;
	clr(good_op);
	while(s[pos] != '[')
		pos++;
	while(s[pos] != ']')
	{
		pos++;
		if (s[pos] != '"')
			throw 42;
		pos++;
		int last = pos + 1;
		while(s[last] != '"')
			last++;
		for(int i = 0; i < (int) operation_names.size(); i++)
			if (operation_names[i] == std::string(s).substr(pos, last - pos))
			{
				//printf("%d %s\n", i, operation_names[i].c_str());
				good_op[i] = 1;
			}
		pos = last + 1;
	}
}

int_vec responses;

void read_responses()
{
	clr(responses);
	gets(s);
	int pos = 0;
	while(s[pos] != '[')
		pos++;
	for(int i = 0; i < max_vals; i++)
	{
		if (s[pos+3] != 'x')
			throw 42;
		pos += 4;
		for(int j = 0; j < 16; j++)
		{
			responses[i] = (responses[i] << 4) + (s[pos] <= '9' ? s[pos] - '0' : s[pos] - 'A' + 10); 
			pos++;
		}
		pos ++;
	}
}

void read_mistakes()
{
	int idx = 0;
	ll temp[3];
	while(gets(s))
	{
		int pos = 0;
		clr(temp);
		while(s[pos] != '[')
			pos++;
		for(int i = 0; i < 3; i++)
		{
			if (s[pos+3] != 'x')
				throw 42;
			pos += 4;
			for(int j = 0; j < 16; j++)
			{
				temp[i] = (temp[i] << 4) + (s[pos] <= '9' ? s[pos] - '0' : s[pos] - 'A' + 10); 
				pos++;
			}
			pos++;
		}
		values[idx] = temp[0];
		responses[idx] = temp[1];
		idx++;
	}
}


int cnt = 0;
bool is_the_same(int k)
{
	memset(values3, 0, sizeof(values3));
	memcpy(values2, values, sizeof(values2));
	for(int s = 0; s < 8; s++)
	{
		eval_lambda(k);
		for(int t = 0; t < max_vals; t++)
			values2[t] >>= 8;
		memcpy(values3, stack[0], sizeof(values3));
	}
	bool same = true;
	for(int t = 0; t < max_vals; t++)
		if (values3[t] != responses[t])
			same = false;
	if (same)
	{
		printf("(lambda (x) (fold x 0 (lambda (y z) ");
		print_tree(k, false);
		printf(")))\n");
		return 1;
	}
	return 0;
}


int find_most_similar()
{
	int max_similarity = 0;
	int ans = 0;
	for(int i = 0; i < cnt; i++)
	{
		int cur_similarity = 0;
		for(int j = 0; j < max_vals; j++)
			if (responses[j] == answers[i][j])
				cur_similarity++;
		if (cur_similarity > max_similarity)
		{
			ans = i;
			max_similarity = cur_similarity;
		}
	}
	printf("Most similar: %d (%d)\n", ans, max_similarity);
	//for(int j = 0; j < max_vals; j++)
		//if (responses[j] != answers[ans][j])
			//printf("%016llX %016llX %016llX\n", values[j], answers[ans][j], responses[j]);
	return ans;
}


int build_if()
{
	printf("Build_if\n");
	int positive = find_most_similar();
	int negative = -1;
	int cond = -1;
	for(int i = 0; i < cnt; i++)
	{
		bool good = true;
		for(int j = 0; j < max_vals; j++)
		{
			if (answers[positive][j] != responses[j] && answers[i][j] != responses[j])
			{
				good = false;
				break;
			}
		}
		if (good)
		{
			negative = i;
			break;
		}
	}
	if (negative == -1)
		return -1;
	for(int i = 0; i < cnt; i++)
	{
		bool good = true;
		for(int j = 0; j < max_vals; j++)
		{
			if (answers[positive][j] != responses[j] && answers[i][j] != 0)
				good = false;
			if (answers[negative][j] != responses[j] && answers[i][j] != 1)
				good = false;
		}
		if (good)
		{
			cond = i;
			break;
		}
	}
	if (cond == -1)
		return -1;
	trees[cnt].rhs = positive;
	trees[cnt].lhs = negative;
	trees[cnt].if_cond = cond;
	trees[cnt].type = if_op;
	return cnt;
}

unsigned int timer = 0;
inline bool test_timer()
{
	return ++timer > 4000000000U;
}

int gen_trees(bool type1)
{
	cnt = 0;
	expr* trees = type1 ? ::trees : trees2;
	for(int i = 0; i < 3; i++)
	{
		trees[cnt].type = i;
		eval(i, type1);
		t[1].push_back(cnt);
		cnt++;
	}
	if (!type1)
	{
		trees[cnt].type = var2;
		eval(cnt, type1);
		cnt++;
		trees[var].type = var3;
	}
	first[1] = 0;
	last[1] = cnt;
	for(int size = 2; size < 21; size++)
	{
		first[size] = cnt;
		for(int type = 3; type < 8; type++)
		{
			if (!good_op[type])
				continue;
			for(int i = first[size-1]; i < last[size-1]; i++)
			{
				trees[cnt].type = type;
				trees[cnt].lhs = i;
				//if (eval(cnt, type1))
				cnt++;
				if (is_the_same(cnt-1))
					return cnt;
				if (test_timer())
					return -1;
				if (cnt >= max_size - 2)
					return -1;
			}
		}
		for(int type = 8; type < 12; type++)
		{
			if (!good_op[type])
				continue;
			for(int left_size = 1; left_size < size - 1; left_size++)
			{
				int right_size = size - 1 - left_size;
				if (left_size < right_size)
					continue;
				for(int i = first[left_size]; i < last[left_size]; i++)
					for(int j = first[right_size]; j < last[right_size] && j <= i; j++)
					{
						trees[cnt].type = type;
						trees[cnt].lhs = i;
						trees[cnt].rhs = j;
						//if (eval(cnt, type1))
						cnt++;
						if (is_the_same(cnt-1))
							return cnt;
						if (test_timer())
							return -1;
						if (cnt >= max_size - 2)
							return -1;
					}
			}
		}
		for(int type = 12; type < 13; type++)
		{
			if (!good_op[type])
				continue;
			for(int left_size = 1; left_size < size - 2; left_size++)
				for(int right_size = 1; right_size + left_size < size - 2; right_size++)
				{
					int last_size = size - 1 - left_size - right_size;
					{
						for(int i = first[left_size]; i < last[left_size]; i++)
							for(int j = first[right_size]; j < last[right_size]; j++)
								for(int k = first[last_size]; k < last[last_size]; k++)
								{
									trees[cnt].type = type;
									trees[cnt].lhs = i;
									trees[cnt].rhs = j;
									trees[cnt].if_cond = k;
									//if (eval(cnt, type1))
									cnt++;
									if (is_the_same(cnt-1))
										return cnt;
									if (test_timer())
										return -1;
									if (cnt >= max_size - 2)
										return -1;
								}
					}
				}
		}
		last[size] = cnt;
		printf("%d %d\n", size, last[size] - first[size]);
		fflush(stdout);
	}
	return -1;
}

int build_fold()
{
	for(int k = 0; k < cnt; k++)
	{
		for(int i = 0; i < cnt; i++)
			for(int j = 0; j < cnt; j++)
			{
				memcpy(values3, answers[i], sizeof(values3));
				memcpy(values2, answers[j], sizeof(values2));
				for(int s = 0; s < 8; s++)
				{
					eval_lambda(k);
					if (ssize != 0)
						throw 42;
					for(int t = 0; t < max_vals; t++)
						values2[t] >>= 8;
					memcpy(values3, stack[0], sizeof(values3));
				}
				bool same = true;
				for(int t = 0; t < max_vals; t++)
					if (values3[t] != responses[t])
						same = false;
				if (same)
				{
					printf("(lambda (x) (fold ");
					print_tree(j);
					printf(" ");
					print_tree(i);
					printf(" (lambda (y z) ");
					print_tree(k, false);
					printf(")))\n");
					return 0;
				}
			}
	}
	return -1;
}

int main()
{
	freopen("input.txt", "r", stdin);
	read_operators();
	//answers = new int_vec[max_size];
	//answers2 = new int_vec[max_size];
	//printf("[");
	for(int i = 0; i < max_vals; i++)
	{
		values[i] = rand64();
		//if (i)
		//printf(",");
		//printf("\"0x%016llX\"", values[i]);
	}
	//for(int i = 0; i < max_vals; i++)
	//values2[i] = rand(0,1) ? (rand(0,3) - 2) & 255 : rand(0,255);
	//printf("]\n");
	read_responses();
	read_mistakes();
	//int similar = gen_trees(true);
	//if (similar != -1)
	//{
		//print_tree_root(similar);
		//return 0;
	//}
	if (gen_trees(false) == -1)
		printf("Fail\n");
	//for(int i = 0; i < cnt; i++)
		//print_tree_root(i);

		//eval(similar);
	
	//similar = build_fold();
	//if (similar != -1)
		//return 0;

	return 0;
}
