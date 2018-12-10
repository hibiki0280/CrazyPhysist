# Copyright 2017 reinforce.io. All Rights Reserved.
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
# ==============================================================================

import numpy as np
import matplotlib.pyplot as plt
import argparse
import sys

from tensorforce.agents import PPOAgent
from tensorforce.execution import Runner
# from tensorforce.contrib.openai_gym import OpenAIGym
# from tensorforce.contrib.unity import UnityEnv
from unity import UnityEnv
import gym

#create directory and return directory path
def create_dir():
    import os
    dir_path = "."
    if "RL_PEDAL_PATH" in os.environ:
        dir_path = os.environ["RL_PEDAL_PATH"]
    import datetime
    datenow = datetime.datetime.now()
    dir_path = dir_path + '/' + str(datenow.year) + '-' + str(datenow.month) + '-' + str(datenow.day) + '-' + str(datenow.hour) + '-' + str(datenow.minute) + '-' + str(datenow.second)
    os.mkdir(dir_path)
    return dir_path

# get parse arguments
parser = argparse.ArgumentParser()
parser.add_argument('-e', '--env', help="Unity environment's path")
args = parser.parse_args()
# Create an Unity environment.
environment = UnityEnv(args.env, worker_id=2)
save_dir = create_dir()
NUM_EPISODES = 1e7
# Network as list of layers
# - Embedding layer:
#   - For Gym environments utilizing a discrete observation space, an
#     "embedding" layer should be inserted at the head of the network spec.
#     Such environments are usually identified by either:
#     - class ...Env(discrete.DiscreteEnv):
#     - self.observation_space = spaces.Discrete(...)

# Note that depending on the following layers used, the embedding layer *may* need a
# flattening layer

network_spec = [
    # dict(type='embedding', indices=100, size=32),
    # dict(type'flatten'),
    dict(type='dense', size=300),
    dict(type='dense', size=200),
    dict(type='dense', size=100)
]
agent = PPOAgent(
    states=environment.states,
    actions=environment.actions,
    network=network_spec,
    # Agent
    states_preprocessing=None,
    actions_exploration=None,
    reward_preprocessing=None,
    # MemoryModel
    update_mode=dict(
        unit='episodes',
        # 10 episodes per update
        batch_size=65536,
        # Every 10 episodes
        frequency=65536
    ),
    memory=dict(
        type='latest',
        include_next_states=False,
        capacity=131072
    ),
    # DistributionModel
    distributions=None,
    entropy_regularization=0.01,
    # PGModel
    baseline_mode='states',
    baseline=dict(
        type='mlp',
        sizes=[32, 32]
    ),
    baseline_optimizer=dict(
        type='multi_step',
        optimizer=dict(
            type='adam',
            learning_rate=1e-4
        ),
        num_steps=5
    ),
    gae_lambda=0.98,
    # PGLRModel
    likelihood_ratio_clipping=0.2,
    # PPOAgent
    step_optimizer=dict(
        type='adam',
        learning_rate=1e-4
    ),
    subsampling_fraction=0.2,
    optimization_steps=25,
    execution=dict(
        type='single',
        session_config=None,
        distributed_spec=None
    )
)

# Create the runner
runner = Runner(agent=agent, environment=environment, repeat_actions=1)


# Callback function printing episode statistics
def episode_finished(r):
    print("Finished episode {ep} after {ts} timesteps (reward: {reward})".format(ep=r.episode, ts=r.episode_timestep,
                                                                                 reward=r.episode_rewards[-1]))
    # record the final episode's model
    if r.episode == NUM_EPISODES: r.agent.save_model(directory=save_dir+"/")
    return True


# Start learning
runner.run(episodes=NUM_EPISODES, max_episode_timesteps=200, episode_finished=episode_finished)

# Print statistics
print("Learning finished. Total episodes: {ep}. Average reward of last 100 episodes: {ar}.".format(
    ep=runner.episode,
    ar=np.mean(runner.episode_rewards[-100:]))
)

s = environment.reset()
while True:
    action = agent.act(s)
    s, fin, reward = environment.execute(action)
    if fin: break

import os
os.environ['KMP_DUPLICATE_LIB_OK']='True'
plt.plot(np.arange(NUM_EPISODES), runner.episode_rewards)
plt.savefig(save_dir+"/learning_curve.png")
np.save(save_dir+"/reward.npy", runner.episode_rewards)
runner.close()
